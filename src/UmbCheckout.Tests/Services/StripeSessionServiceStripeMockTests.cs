using UmbCheckout.Testing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Stripe.Checkout;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Tests.Infrastructure;
using UmbHost.Licensing.Services;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    /// <summary>Checkout end to end through StripeSessionService, against stripe-mock.</summary>
    [Trait("Category", "Integration")]
    [Collection("stripe-mock")]
    public class StripeSessionServiceStripeMockTests
    {
        private readonly StripeMockFixture _fixture;
        private readonly CapturingHandler _handler = new();
        private readonly Mock<IUmbCheckoutStripeClientFactory> _factory = new();
        private readonly Mock<IStripeShippingOptionsService> _shippingOptions = new();
        private readonly Mock<IEventAggregator> _events = new();

        public StripeSessionServiceStripeMockTests(StripeMockFixture fixture) => _fixture = fixture;

        private StripeSessionService Service(Guid productKey, bool enableShipping = false)
        {
            var product = new Mock<IPublishedContent> { DefaultValue = DefaultValue.Mock };
            product.Setup(p => p.Key).Returns(productKey);
            product.Setup(p => p.Name).Returns("Ping Pong Ball");
            var contentCache = new Mock<IPublishedContentCache>();
            contentCache.Setup(c => c.GetById(productKey)).Returns(product.Object);

            _factory.Setup(f => f.CreateAsync()).ReturnsAsync(() => _fixture.CreateClient(_handler));
            _factory.Setup(f => f.CreateAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => _fixture.CreateClient(_handler));
            _shippingOptions.Setup(s => s.GetShippingOptions(It.IsAny<Basket>())).ReturnsAsync([]);
            _shippingOptions.Setup(s => s.GetShippingOptions(It.IsAny<Basket>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

            // No Stripe settings row saved yet (a fresh install): checkout must still use the configured test key.
            var stripeSettingsService = new Mock<IStripeSettingsService>();
            stripeSettingsService.Setup(s => s.GetStripeSettingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync((UmbCheckout.Stripe.Models.UmbCheckoutStripeSettings?)null);

            // The runtime level is not Run, so the licence check returns before using its other dependencies.
            var licenseService = UmbCheckout.Testing.TestLicenseService.None;

            return new StripeSessionService(
                contentCache.Object,
                new Mock<ICoreScopeProvider> { DefaultValue = DefaultValue.Mock }.Object,
                _events.Object,
                NullLogger<StripeSessionService>.Instance,
                Mock.Of<IConfigurationService>(c => c.GetConfigurationAsync(It.IsAny<CancellationToken>()) == Task.FromResult<UmbCheckoutConfiguration?>(enableShipping ? new UmbCheckoutConfiguration { EnableShipping = true } : null)),
                Mock.Of<IHttpContextAccessor>(),
                _shippingOptions.Object,
                licenseService,
                stripeSettingsService.Object,
                _factory.Object);
        }

        [Fact]
        public async Task A_cancelled_request_stops_the_Stripe_call()
        {
            // Quality Phase 5: a shopper who leaves mid-checkout doesn't keep a Stripe call (and a thread) busy.
            var key = Guid.NewGuid();
            var basket = new Basket { SiteRootCulture = "en-GB", CurrencyCode = "GBP", LineItems = [new LineItem { Key = key, Name = "Ping Pong Ball", Price = 2m, CurrencyCode = "GBP", Quantity = 1 }] };
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Service(key).CreateSessionAsync(basket, cancelled.Token));

            Assert.Empty(_handler.RequestUris);
        }

        [Fact]
        public async Task The_request_token_reaches_the_client_factory_and_the_shipping_options()
        {
            var key = Guid.NewGuid();
            var basket = new Basket { SiteRootCulture = "en-GB", CurrencyCode = "GBP", LineItems = [new LineItem { Key = key, Name = "Ping Pong Ball", Description = "Table tennis ball", Price = 2m, CurrencyCode = "GBP", Quantity = 1 }] };
            using var request = new CancellationTokenSource();

            await Service(key, enableShipping: true).CreateSessionAsync(basket, request.Token);

            _factory.Verify(f => f.CreateAsync(request.Token), Times.Once);
            _shippingOptions.Verify(s => s.GetShippingOptions(basket, request.Token), Times.Once);
        }

        [Theory]
        [InlineData(nameof(IStripeSessionService.GetSession))]
        [InlineData(nameof(IStripeSessionService.CreateSession))]
        [InlineData(nameof(IStripeSessionService.ClearSession))]
        public void The_blocking_methods_are_obsolete(string method)
            => Assert.All(typeof(IStripeSessionService).GetMethods().Where(m => m.Name == method),
                m => Assert.NotNull(m.GetCustomAttributes(typeof(ObsoleteAttribute), false).SingleOrDefault()));
        [Fact]
        public async Task CreateSessionAsync_WithNoSavedStripeSettings_ChargesTheBasketInMinorUnits()
        {
            var key = Guid.NewGuid();
            var basket = new Basket { SiteRootCulture = "en-GB", CurrencyCode = "GBP", LineItems = [new LineItem { Key = key, Name = "Ping Pong Ball", Description = "Table tennis ball", Price = 2m, CurrencyCode = "GBP", Quantity = 2 }] };

            await Service(key).CreateSessionAsync(basket);

            var body = Assert.Single(_handler.RequestBodies);
            Assert.Contains("/v1/checkout/sessions", Assert.Single(_handler.RequestUris));
            Assert.Contains("line_items[0][price_data][unit_amount]=200", body);
            Assert.Contains("line_items[0][price_data][currency]=gbp", body);
            Assert.Contains("line_items[0][quantity]=2", body);
            Assert.Contains($"line_items[0][price_data][product_data][metadata][nodeKey]={key}", body);
            // Which basket this session pays for, so the success page clears it.
            Assert.Contains("&metadata[umbcheckout_culture]=en-GB", body);
            Assert.Contains("&metadata[umbcheckout_currency]=GBP", body);
        }

        [Fact]
        public async Task CreateSessionAsync_WithNoShippingRateForTheBasketsCurrency_SendsNoShippingOptions()
        {
            // Shipping is on but no configured rate charges in this basket's currency: checkout must still go ahead.
            var key = Guid.NewGuid();
            var basket = new Basket { SiteRootCulture = "en-GB", CurrencyCode = "GBP", LineItems = [new LineItem { Key = key, Name = "Ping Pong Ball", Description = "Table tennis ball", Price = 2m, CurrencyCode = "GBP", Quantity = 1 }] };

            await Service(key, enableShipping: true).CreateSessionAsync(basket);

            Assert.DoesNotContain("shipping_options", Assert.Single(_handler.RequestBodies));
        }

        [Fact]
        public async Task A_session_option_handlers_change_is_sent_and_accepted()
        {
            // Handlers run before the session is created (they were queued until the scope ended), and Stripe accepts what they set.
            _events.Setup(e => e.PublishAsync(It.IsAny<UmbCheckout.Stripe.Notifications.OnProviderSessionOptionsStarted>(), It.IsAny<CancellationToken>()))
                .Callback<UmbCheckout.Stripe.Notifications.OnProviderSessionOptionsStarted, CancellationToken>((n, _) => n.Options.Metadata["order_reference"] = "order-42")
                .Returns(Task.CompletedTask);
            var key = Guid.NewGuid();
            var basket = new Basket { SiteRootCulture = "en-GB", CurrencyCode = "GBP", LineItems = [new LineItem { Key = key, Name = "Ping Pong Ball", Description = "Table tennis ball", Price = 2m, CurrencyCode = "GBP", Quantity = 1 }] };

            var session = await Service(key).CreateSessionAsync(basket, CancellationToken.None);

            Assert.NotNull(session.Id);
            Assert.Contains("metadata[order_reference]=order-42", Assert.Single(_handler.RequestBodies));
        }
    }
}
