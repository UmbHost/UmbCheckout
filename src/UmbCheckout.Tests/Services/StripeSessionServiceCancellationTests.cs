using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Stripe;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Models;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Tests.Helpers;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    /// <summary>
    /// Group 6: the checkout session service passes the caller's token to Stripe. A cancellation the caller asked for (the customer
    /// left) stays silent; any other cancellation is Stripe's HTTP timeout and is logged as a Warning, not an Error. Checkout reads
    /// the configuration and Stripe settings before opening its scope, so the settings cache is used and filled.
    /// </summary>
    public sealed class StripeSessionServiceCancellationTests
    {
        /// <summary>Stripe never answers: the HTTP client times out (a cancellation nobody asked for), or the caller cancels meanwhile.</summary>
        private sealed class TimingOutHandler : HttpMessageHandler
        {
            private readonly CancellationTokenSource? _callerCancels;

            public TimingOutHandler(CancellationTokenSource? callerCancels = null) => _callerCancels = callerCancels;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                _callerCancels?.Cancel();
                throw new TaskCanceledException("The request was canceled due to the configured HttpClient.Timeout");
            }
        }

        private readonly List<string> _order = new();
        private readonly RecordingLogger<StripeSessionService> _logger = new();

        private StripeSessionService Service(HttpMessageHandler handler)
        {
            var scope = new Mock<ICoreScope>();
            scope.SetupGet(s => s.Notifications).Returns(Mock.Of<IScopedNotificationPublisher>());
            var scopes = new Mock<ICoreScopeProvider>();
            scopes.Setup(p => p.CreateCoreScope(It.IsAny<IsolationLevel>(), It.IsAny<RepositoryCacheMode>(), It.IsAny<IEventDispatcher?>(),
                    It.IsAny<IScopedNotificationPublisher?>(), It.IsAny<bool?>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Callback(() => _order.Add("scope"))
                .Returns(scope.Object);

            var configuration = new Mock<IConfigurationService>();
            configuration.Setup(c => c.GetConfiguration()).Callback(() => _order.Add("configuration")).ReturnsAsync(new UmbCheckoutConfiguration());
            var stripeSettings = new Mock<IStripeSettingsService>();
            stripeSettings.Setup(s => s.GetStripeSettings()).Callback(() => _order.Add("stripe settings")).ReturnsAsync(new UmbCheckoutStripeSettings());

            var client = new StripeClient("sk_test_fake", httpClient: new SystemNetHttpClient(new HttpClient(handler), maxNetworkRetries: 0));
            var factory = new Mock<IUmbCheckoutStripeClientFactory>();
            factory.Setup(f => f.CreateAsync(It.IsAny<CancellationToken>())).ReturnsAsync(client);

            return new StripeSessionService(Mock.Of<IPublishedSnapshotAccessor>(), scopes.Object, Mock.Of<IEventAggregator>(), _logger, configuration.Object,
                Mock.Of<IHttpContextAccessor>(), Mock.Of<IStripeShippingOptionsService>(), Mock.Of<IOptionsMonitor<StripeSettings>>(o => o.CurrentValue == new StripeSettings()),
                null!, stripeSettings.Object, factory.Object);
        }

        [Fact]
        public async Task A_Stripe_timeout_is_logged_as_a_warning()
        {
            var service = Service(new TimingOutHandler());

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetSessionAsync("cs_test_1", CancellationToken.None));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ClearSessionAsync("cs_test_1", CancellationToken.None));

            Assert.Equal(2, _logger.Count(LogLevel.Warning));
            Assert.Equal(0, _logger.Count(LogLevel.Error));
            Assert.All(_logger.Entries, e => Assert.Contains("timed out", e.Message));
        }

        [Fact]
        public async Task A_cancellation_the_caller_asked_for_is_silent()
        {
            using var request = new CancellationTokenSource();
            var service = Service(new TimingOutHandler(request));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetSessionAsync("cs_test_1", request.Token));

            Assert.Empty(_logger.Entries);
        }

        [Fact]
        public async Task A_request_already_cancelled_never_reaches_Stripe()
        {
            using var request = new CancellationTokenSource();
            request.Cancel();
            var service = Service(new TimingOutHandler());

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.CreateSessionAsync(new Basket(), request.Token));

            Assert.Empty(_order);
            Assert.Empty(_logger.Entries);
        }

        [Fact]
        public async Task Checkout_reads_the_settings_before_opening_its_scope()
        {
            var service = Service(new TimingOutHandler());

            await Assert.ThrowsAnyAsync<Exception>(() => service.CreateSessionAsync(new Basket { CurrencyCode = "GBP" }, CancellationToken.None));

            Assert.Equal(new[] { "configuration", "stripe settings", "scope" }, _order.Take(3));
        }
    }
}
