using System.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Services;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Models;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Tests.Infrastructure;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Xunit;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace UmbCheckout.Tests.Services
{
    /// <summary>
    /// The token-taking overloads pass the caller's token down to the database (configuration, settings, shipping rates) and the
    /// next service, so a shopper who leaves stops the work. Each dependency here fails with OperationCanceledException only when
    /// it is handed the cancelled token.
    /// </summary>
    public class CancellationTokenTests
    {
        private readonly CancellationToken _cancelled = new(canceled: true);

        /// <summary>A result for a token-taking setup: cancelled when given a cancelled token.</summary>
        private static Task<T> Honour<T>(CancellationToken cancellationToken, T value)
            => cancellationToken.IsCancellationRequested ? Task.FromCanceled<T>(cancellationToken) : Task.FromResult(value);

        private static Mock<IConfigurationService> Configuration()
        {
            var configuration = new Mock<IConfigurationService>();
            configuration.Setup(c => c.GetConfigurationAsync(It.IsAny<CancellationToken>()))
                .Returns((CancellationToken t) => Honour<UmbCheckoutConfiguration?>(t, new UmbCheckoutConfiguration { CurrencyCode = "GBP" }));
            return configuration;
        }

        private static SessionService SessionService(Mock<IConfigurationService> configuration)
        {
            var basketContext = new Mock<IBasketContext>();
            basketContext.Setup(b => b.SiteRootCulture).Returns("en-GB");
            return new SessionService(
                new EphemeralDataProtectionProvider(),
                Mock.Of<IHttpContextAccessor>(a => a.HttpContext == new DefaultHttpContext { Session = new InMemorySession() }),
                NullLogger<SessionService>.Instance,
                Mock.Of<IEventAggregator>(),
                new Mock<ICoreScopeProvider> { DefaultValue = DefaultValue.Mock }.Object,
                configuration.Object,
                UmbCheckout.Testing.TestLicenseService.Create(),
                basketContext.Object,
                new UmbCheckout.Testing.TestEventMessagesFactory(),
                Mock.Of<IPaidCheckoutSessions>());
        }

        [Fact]
        public async Task Session_reads_pass_the_token_to_the_configuration_read()
        {
            ISessionService service = SessionService(Configuration());

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.Get("en-GB", "GBP", _cancelled));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.Get("GBP", _cancelled));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.Update(new Basket { CurrencyCode = "GBP" }, _cancelled));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.Clear("en-GB", "GBP", _cancelled));
        }

        [Fact]
        public async Task Session_reads_without_a_token_still_work()
        {
            ISessionService service = SessionService(Configuration());

            Assert.NotNull(await service.Get("en-GB", "GBP"));
        }

        [Fact]
        public async Task Basket_changes_pass_the_token_to_the_session()
        {
            var session = new UmbCheckoutSession { Basket = new Basket { CurrencyCode = "GBP" } };
            var sessions = new Mock<ISessionService>();
            sessions.Setup(s => s.Get(It.IsAny<CancellationToken>())).Returns((CancellationToken t) => Honour(t, session));
            sessions.Setup(s => s.Get(It.IsAny<string?>(), It.IsAny<CancellationToken>())).Returns((string? _, CancellationToken t) => Honour(t, session));
            sessions.Setup(s => s.Update(It.IsAny<Basket>(), It.IsAny<CancellationToken>())).Returns((Basket _, CancellationToken t) => Honour(t, session));
            var scope = new Mock<IScope>();
            scope.Setup(x => x.Notifications).Returns(Mock.Of<IScopedNotificationPublisher>());
            var scopes = new Mock<ICoreScopeProvider>();
            scopes.Setup(x => x.CreateCoreScope(IsolationLevel.Unspecified, RepositoryCacheMode.Unspecified, null, null, null, false, true)).Returns(scope.Object);
            IBasketService service = new BasketService(sessions.Object, NullLogger<BasketService>.Instance, Mock.Of<IEventAggregator>(), scopes.Object, new UmbCheckout.Testing.TestEventMessagesFactory());
            var line = new LineItem { Key = Guid.NewGuid(), Quantity = 1, Price = 1m, CurrencyCode = "GBP" };

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.Get(_cancelled));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.Add(line, _cancelled));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.Add([line], _cancelled));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.Reduce(line.Key, _cancelled));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.Remove(line.Key, _cancelled));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.Remove([line.Key], _cancelled));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.Clear(_cancelled));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.TotalItems(_cancelled));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.SubTotal(_cancelled));
        }

        [Fact]
        public async Task Currency_lookups_pass_the_token_to_the_configuration_read()
        {
            var configuration = Configuration();
            ICurrencyService currency = new CurrencyService(configuration.Object, NullLogger<CurrencyService>.Instance, Mock.Of<IUmbracoContextAccessor>(),
                Mock.Of<IDocumentNavigationQueryService>(), Mock.Of<IPublishedContentCache>());
            IBasketContext basketContext = new UmbracoBasketContext(Mock.Of<IUmbracoContextAccessor>(), currency, configuration.Object);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => currency.GetCurrencyForContentAsync(Mock.Of<IPublishedContent>(), _cancelled));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => currency.GetCurrencyAsync(null, _cancelled));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => basketContext.GetCurrencyAsync(_cancelled));
        }

        [Fact]
        public async Task The_Stripe_client_factory_passes_the_token_to_the_settings_read()
        {
            var settings = new Mock<IStripeSettingsService>();
            settings.Setup(s => s.GetStripeSettingsAsync(It.IsAny<CancellationToken>())).Returns((CancellationToken t) => Honour<UmbCheckoutStripeSettings?>(t, null));
            var options = Mock.Of<IOptionsMonitor<StripeSettings>>(o => o.CurrentValue == new StripeSettings { Test = new StripeApiDetails { ApiKey = "sk_test_key" } });
            IUmbCheckoutStripeClientFactory factory = new UmbCheckoutStripeClientFactory(options, settings.Object);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => factory.CreateAsync(_cancelled));
        }

        [Fact]
        public async Task Pricing_passes_the_token_to_the_currency_lookup_and_on_to_each_line()
        {
            var currency = new Mock<ICurrencyService>();
            currency.Setup(c => c.GetCurrencyForContentAsync(It.IsAny<IPublishedContent>(), It.IsAny<CancellationToken>()))
                .Returns((IPublishedContent _, CancellationToken t) => Honour<string?>(t, "GBP"));
            var price = new Mock<IPublishedProperty>();
            price.Setup(p => p.HasValue(It.IsAny<string?>(), It.IsAny<string?>())).Returns(true);
            price.Setup(p => p.GetValue(It.IsAny<string?>(), It.IsAny<string?>())).Returns(10m);
            var product = new Mock<IPublishedContent>();
            product.Setup(p => p.GetProperty(UmbCheckout.Shared.Consts.PropertyAlias.PriceAlias)).Returns(price.Object);
            IStripeLineItemPricingService pricing = new StripeLineItemPricingService(currency.Object, Mock.Of<IBasketContext>(), NullLogger<StripeLineItemPricingService>.Instance);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pricing.GetPriceAsync(product.Object, _cancelled));

            var line = new LineItem { Key = Guid.NewGuid(), Quantity = 1, Price = 10m, CurrencyCode = "GBP" };
            var cache = new Mock<IPublishedContentCache>();
            cache.Setup(c => c.GetByIdAsync(line.Key, It.IsAny<bool?>())).ReturnsAsync(product.Object);
            IStripeBasketPricingService basketPricing = new StripeBasketPricingService(cache.Object, pricing, NullLogger<StripeBasketPricingService>.Instance);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => basketPricing.RepriceAsync(new Basket { CurrencyCode = "GBP", LineItems = [line] }, _cancelled));
        }

        /// <summary>A site's own implementation written before the token overloads existed.</summary>
        private sealed class OldShippingOptions : IStripeShippingOptionsService
        {
            public Task<List<global::Stripe.Checkout.SessionShippingOptionOptions>> GetShippingOptions(Basket basket)
                => Task.FromResult(new List<global::Stripe.Checkout.SessionShippingOptionOptions> { new() { ShippingRate = "shr_site" } });
        }

        [Fact]
        public async Task An_implementation_without_the_token_overload_still_compiles_and_is_called()
        {
            IStripeShippingOptionsService service = new OldShippingOptions();

            Assert.Equal("shr_site", Assert.Single(await service.GetShippingOptions(new Basket(), CancellationToken.None)).ShippingRate);
        }

        [Fact]
        public async Task Shipping_options_pass_the_token_to_the_shipping_rates_read_and_Stripe()
        {
            var database = new Mock<IStripeShippingRateDatabaseService>();
            database.Setup(d => d.GetShippingRatesAsync(It.IsAny<CancellationToken>()))
                .Returns((CancellationToken t) => Honour<IEnumerable<ShippingRate>>(t, [new ShippingRate { Value = "shr_1" }]));
            var api = new Mock<IStripeShippingRateApiService>();
            CancellationToken seen = default;
            api.Setup(a => a.GetShippingRate("shr_1", It.IsAny<CancellationToken>()))
                .Callback((string _, CancellationToken t) => seen = t)
                .ReturnsAsync(new global::Stripe.ShippingRate { Id = "shr_1", Active = true, FixedAmount = new global::Stripe.ShippingRateFixedAmount { Currency = "gbp" } });
            IStripeShippingOptionsService service = new StripeDefaultShippingOptionsService(database.Object, api.Object, NullLogger<StripeDefaultShippingOptionsService>.Instance);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetShippingOptions(new Basket { CurrencyCode = "GBP" }, _cancelled));

            using var request = new CancellationTokenSource();
            Assert.Single(await service.GetShippingOptions(new Basket { CurrencyCode = "GBP" }, request.Token));
            Assert.Equal(request.Token, seen);
        }
    }
}
