using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Services;
using UmbCheckout.Shared.Helpers;
using UmbCheckout.Shared.Models;
using UmbCheckout.Tests.Infrastructure;
using UmbHost.Licensing.Services;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Xunit;

namespace UmbCheckout.Tests
{
    /// <summary>A basket belongs to exactly one currency: one session basket per site root culture and currency.</summary>
    public class SessionServiceTests
    {
        private readonly InMemorySession _session = new();
        private readonly Mock<IBasketContext> _basketContext = new();

        private SessionService Service()
        {
            var httpContext = new DefaultHttpContext { Session = _session };
            _basketContext.Setup(b => b.SiteRootCulture).Returns("en-GB");
            var licenseService = new LicenseService(null!, NullLogger<LicenseService>.Instance, null!, Options.Create(new UmbHost.Licensing.Models.UmbCheckoutAppSettings()), null!, Mock.Of<IRuntimeState>(), Options.Create(new WebRoutingSettings()), null!);

            return new SessionService(
                new EphemeralDataProtectionProvider(),
                Mock.Of<IHttpContextAccessor>(a => a.HttpContext == httpContext),
                NullLogger<SessionService>.Instance,
                Mock.Of<IEventAggregator>(),
                new Mock<ICoreScopeProvider> { DefaultValue = DefaultValue.Mock }.Object,
                Mock.Of<IConfigurationService>(),
                licenseService,
                _basketContext.Object);
        }

        private static LineItem Line(string currency) => new() { Key = Guid.NewGuid(), CurrencyCode = currency, Price = 1m, Quantity = 1 };

        [Fact]
        public async Task Get_KeepsASeparateBasketPerCurrency()
        {
            var service = Service();
            var gbp = Line("GBP");
            await service.Update(new Basket { CurrencyCode = "GBP", LineItems = [gbp] });

            var usd = await service.Get("USD");
            var pounds = await service.Get("GBP");

            Assert.Empty(usd.Basket.LineItems);
            Assert.Equal("USD", usd.Basket.CurrencyCode);
            Assert.Equal(gbp.Key, Assert.Single(pounds.Basket.LineItems).Key);
        }

        [Fact]
        public async Task Get_UsesTheRequestsCurrencyByDefault()
        {
            _basketContext.Setup(b => b.GetCurrencyAsync()).ReturnsAsync("EUR");

            Assert.Equal("EUR", (await Service().Get()).Basket.CurrencyCode);
        }

        [Fact]
        public async Task Get_SplitsALegacyBasketByCurrency()
        {
            var gbp = Line("GBP");
            var usd = Line("USD");
            _session.SetObjectAsJson(BasketKeys.LegacySession("en-GB"), new UmbCheckoutSession
            {
                SiteRootCulture = "en-GB",
                Basket = new Basket { SiteRootCulture = "en-GB", LineItems = [gbp, usd, Line("")] }
            });
            var service = Service();

            Assert.Equal(gbp.Key, Assert.Single((await service.Get("GBP")).Basket.LineItems).Key);
            Assert.Equal(usd.Key, Assert.Single((await service.Get("USD")).Basket.LineItems).Key);
            Assert.DoesNotContain(BasketKeys.LegacySession("en-GB"), _session.Keys);
        }

        [Fact]
        public async Task Clear_RemovesOnlyThatCurrencysBasket()
        {
            var service = Service();
            await service.Update(new Basket { CurrencyCode = "GBP", LineItems = [Line("GBP")] });
            await service.Update(new Basket { CurrencyCode = "USD", LineItems = [Line("USD")] });

            await service.Clear("GBP");

            Assert.Empty((await service.Get("GBP")).Basket.LineItems);
            Assert.Single((await service.Get("USD")).Basket.LineItems);
        }
    }
}
