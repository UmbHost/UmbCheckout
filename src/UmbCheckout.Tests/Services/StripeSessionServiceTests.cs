using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Models;
using UmbCheckout.Stripe.Services;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    /// <summary>StripeSessionService without Stripe: logging of timeouts and where it reads its settings.</summary>
    public sealed class StripeSessionServiceTests
    {
        private readonly List<string> _calls = [];
        private readonly RecordingLogger _logger = new();
        private readonly Mock<IUmbCheckoutStripeClientFactory> _factory = new();

        private sealed class RecordingLogger : ILogger<StripeSessionService>
        {
            public List<LogLevel> Levels { get; } = [];

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
                => Levels.Add(logLevel);
        }

        private StripeSessionService Service()
        {
            var scopes = new Mock<ICoreScopeProvider> { DefaultValue = DefaultValue.Mock };
            scopes.Setup(s => s.CreateCoreScope(It.IsAny<System.Data.IsolationLevel>(), It.IsAny<Umbraco.Cms.Core.Scoping.RepositoryCacheMode>(),
                    It.IsAny<IEventDispatcher?>(), It.IsAny<IScopedNotificationPublisher?>(), It.IsAny<bool?>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Callback(() => _calls.Add("scope"))
                .Returns(() => new Mock<ICoreScope> { DefaultValue = DefaultValue.Mock }.Object);

            var configuration = new Mock<IConfigurationService>();
            configuration.Setup(c => c.GetConfigurationAsync(It.IsAny<CancellationToken>())).Callback(() => _calls.Add("configuration")).ReturnsAsync((UmbCheckoutConfiguration?)null);
            var stripeSettings = new Mock<IStripeSettingsService>();
            stripeSettings.Setup(s => s.GetStripeSettingsAsync(It.IsAny<CancellationToken>())).Callback(() => _calls.Add("stripe settings")).ReturnsAsync((UmbCheckoutStripeSettings?)null);

            return new StripeSessionService(Mock.Of<IPublishedContentCache>(), scopes.Object, Mock.Of<IEventAggregator>(), _logger, configuration.Object,
                Mock.Of<IHttpContextAccessor>(), Mock.Of<IStripeShippingOptionsService>(), UmbCheckout.Testing.TestLicenseService.None, stripeSettings.Object,
                _factory.Object);
        }

        private static Basket Basket() => new() { CurrencyCode = "GBP", LineItems = [new LineItem { Key = Guid.NewGuid(), Name = "Ball", Price = 2m, CurrencyCode = "GBP", Quantity = 1 }] };

        [Fact]
        public async Task A_Stripe_timeout_is_logged_as_a_warning()
        {
            // Review minor: every OperationCanceledException went unlogged, so Stripe HTTP timeouts left no trace at service level.
            _factory.Setup(f => f.CreateAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new TaskCanceledException("The request timed out"));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Service().GetSessionAsync("cs_test_1", CancellationToken.None));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Service().ClearSessionAsync("cs_test_1", CancellationToken.None));

            Assert.Equal([LogLevel.Warning, LogLevel.Warning], _logger.Levels);
        }

        [Fact]
        public async Task A_cancelled_request_stays_silent()
        {
            using var cancelled = new CancellationTokenSource();
            _factory.Setup(f => f.CreateAsync(It.IsAny<CancellationToken>())).Callback(cancelled.Cancel).ThrowsAsync(new OperationCanceledException(cancelled.Token));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Service().GetSessionAsync("cs_test_1", cancelled.Token));

            Assert.Empty(_logger.Levels);
        }

        [Fact]
        public async Task Checkout_reads_its_settings_before_opening_a_scope_so_they_can_be_cached()
        {
            // Review minor: reads inside the service's own scope were never cached (a read in an outer scope may yet roll back).
            _factory.Setup(f => f.CreateAsync(It.IsAny<CancellationToken>())).Callback(() => _calls.Add("stripe client")).ThrowsAsync(new InvalidOperationException("stop here"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => Service().CreateSessionAsync(Basket(), CancellationToken.None));

            Assert.Equal(["configuration", "stripe settings", "scope", "stripe client"], _calls);
        }
    }
}
