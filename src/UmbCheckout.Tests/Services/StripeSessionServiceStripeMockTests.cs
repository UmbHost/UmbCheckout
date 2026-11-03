using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Models;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Testing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    /// <summary>Checkout end to end through StripeSessionService, against stripe-mock (Docker).</summary>
    [Trait("Category", "Integration")]
    [Collection("stripe-mock")]
    public class StripeSessionServiceStripeMockTests
    {
        private readonly StripeMockFixture _fixture;
        private readonly CapturingHandler _handler = new();
        private readonly Mock<IUmbCheckoutStripeClientFactory> _factory = new();
        private readonly Mock<IStripeShippingOptionsService> _shippingOptions = new();
        private readonly Mock<IEventAggregator> _events = new();

        public StripeSessionServiceStripeMockTests(StripeMockFixture fixture)
        {
            _fixture = fixture;
            // The HasValue/Value extensions the options builder uses resolve these from Umbraco's static service provider.
            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<IPublishedValueFallback>());
            services.AddSingleton(Mock.Of<Umbraco.Cms.Core.Services.IMemberTypeService>());
            services.AddSingleton(Mock.Of<Umbraco.Cms.Core.Services.IMediaTypeService>());
            services.AddSingleton(Mock.Of<Umbraco.Cms.Core.Services.IContentTypeService>());
            services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new Umbraco.Cms.Core.Configuration.Models.WebRoutingSettings()));
            services.AddSingleton(Mock.Of<Umbraco.Cms.Core.Services.IFileService>());
            services.AddSingleton(Mock.Of<Examine.IExamineManager>());
            services.AddSingleton(Mock.Of<Umbraco.Cms.Core.Routing.ISiteDomainMapper>());
            services.AddSingleton(Mock.Of<Umbraco.Cms.Core.Web.IUmbracoContextAccessor>());
            services.AddSingleton(Mock.Of<Umbraco.Cms.Core.Services.IUserService>());
            services.AddSingleton(Mock.Of<Umbraco.Cms.Core.Routing.IPublishedUrlProvider>());
            services.AddSingleton(Mock.Of<IPublishedModelFactory>());
            services.AddSingleton(Mock.Of<IVariationContextAccessor>());
            services.AddSingleton(Mock.Of<IPublishedSnapshotAccessor>());
            StaticServiceProvider.Instance = services.BuildServiceProvider();
        }

        private StripeSessionService Service(Guid productKey, bool enableShipping = false)
        {
            var product = new Mock<IPublishedContent> { DefaultValue = DefaultValue.Mock };
            product.Setup(p => p.Key).Returns(productKey);
            product.Setup(p => p.Name).Returns("Ping Pong Ball");
            var content = new Mock<IPublishedContentCache>();
            content.Setup(c => c.GetById(productKey)).Returns(product.Object);
            var snapshot = new Mock<IPublishedSnapshot>();
            snapshot.Setup(s => s.Content).Returns(content.Object);
            var published = snapshot.Object;
            var snapshotAccessor = new Mock<IPublishedSnapshotAccessor>();
            snapshotAccessor.Setup(a => a.TryGetPublishedSnapshot(out published)).Returns(true);

            _factory.Setup(f => f.CreateAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => _fixture.CreateClient(_handler));
            _shippingOptions.Setup(s => s.GetShippingOptions(It.IsAny<Basket>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<global::Stripe.Checkout.SessionShippingOptionOptions>());

            var stripeSettingsService = new Mock<IStripeSettingsService>();
            stripeSettingsService.Setup(s => s.GetStripeSettings()).ReturnsAsync((UmbCheckoutStripeSettings?)null);

            return new StripeSessionService(
                snapshotAccessor.Object,
                new Mock<ICoreScopeProvider> { DefaultValue = DefaultValue.Mock }.Object,
                _events.Object,
                NullLogger<StripeSessionService>.Instance,
                Mock.Of<IConfigurationService>(c => c.GetConfiguration() == Task.FromResult<UmbCheckoutConfiguration?>(enableShipping ? new UmbCheckoutConfiguration { EnableShipping = true } : null)),
                Mock.Of<IHttpContextAccessor>(),
                _shippingOptions.Object,
                Mock.Of<IOptionsMonitor<StripeSettings>>(o => o.CurrentValue == new StripeSettings()),
                null, // the licence service is only used for live sessions
                stripeSettingsService.Object,
                _factory.Object);
        }

        private static Basket Basket(Guid key, int quantity = 1) => new Basket
        {
            SiteRootCulture = "en-GB",
            CurrencyCode = "GBP",
            LineItems = new List<LineItem> { new() { Key = key, Name = "Ping Pong Ball", Description = "Table tennis ball", Price = 2m, CurrencyCode = "GBP", Quantity = quantity } }
        };

        [Fact]
        public async Task A_cancelled_request_stops_the_Stripe_call()
        {
            var key = Guid.NewGuid();
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Service(key).CreateSessionAsync(Basket(key), cancelled.Token));

            Assert.Empty(_handler.RequestUris);
        }

        [Fact]
        public async Task The_request_token_reaches_the_client_factory_and_the_shipping_options()
        {
            var key = Guid.NewGuid();
            var basket = Basket(key);
            using var request = new CancellationTokenSource();

            await Service(key, enableShipping: true).CreateSessionAsync(basket, request.Token);

            _factory.Verify(f => f.CreateAsync(request.Token), Times.Once);
            _shippingOptions.Verify(s => s.GetShippingOptions(basket, request.Token), Times.Once);
        }

        [Fact]
        public async Task CreateSessionAsync_WithNoSavedStripeSettings_ChargesTheBasketInMinorUnits()
        {
            var key = Guid.NewGuid();

            await Service(key).CreateSessionAsync(Basket(key, quantity: 2), CancellationToken.None);

            var body = Assert.Single(_handler.RequestBodies);
            Assert.Contains("/v1/checkout/sessions", Assert.Single(_handler.RequestUris));
            Assert.Contains("line_items[0][price_data][unit_amount]=200", body);
            Assert.Contains("line_items[0][price_data][currency]=gbp", body);
            Assert.Contains("line_items[0][quantity]=2", body);
            Assert.Contains($"line_items[0][price_data][product_data][metadata][nodeKey]={key}", body);
            Assert.Contains("metadata[umbcheckout_culture]=en-GB", body);
            Assert.Contains("metadata[umbcheckout_currency]=GBP", body);
        }

        [Fact]
        public async Task CreateSessionAsync_WithNoShippingRateForTheBasketsCurrency_SendsNoShippingOptions()
        {
            var key = Guid.NewGuid();

            await Service(key, enableShipping: true).CreateSessionAsync(Basket(key), CancellationToken.None);

            Assert.DoesNotContain("shipping_options", Assert.Single(_handler.RequestBodies));
        }

        [Fact]
        public async Task A_session_option_handlers_change_is_sent_and_accepted()
        {
            _events.Setup(e => e.PublishAsync(It.IsAny<UmbCheckout.Stripe.Notifications.OnProviderSessionOptionsStarted>(), It.IsAny<CancellationToken>()))
                .Callback<UmbCheckout.Stripe.Notifications.OnProviderSessionOptionsStarted, CancellationToken>((n, _) => n.Options.Metadata["order_reference"] = "order-42")
                .Returns(Task.CompletedTask);
            var key = Guid.NewGuid();

            var session = await Service(key).CreateSessionAsync(Basket(key), CancellationToken.None);

            Assert.NotNull(session.Id);
            Assert.Contains("metadata[order_reference]=order-42", Assert.Single(_handler.RequestBodies));
        }
    }
}
