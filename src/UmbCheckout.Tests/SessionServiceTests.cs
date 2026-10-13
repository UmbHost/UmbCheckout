using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UmbCheckout.Core.Composers;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Services;
using UmbCheckout.Shared.Helpers;
using UmbCheckout.Shared.Models;
using UmbCheckout.Shared.Notifications.Session;
using UmbCheckout.Tests.Infrastructure;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Web;
using Xunit;

namespace UmbCheckout.Tests
{
    /// <summary>A basket belongs to exactly one currency: one session basket per site root culture and currency.</summary>
    public class SessionServiceTests
    {
        private readonly InMemorySession _session = new();
        private readonly Mock<IBasketContext> _basketContext = new();
        private readonly List<INotification> _published = new();

        public SessionServiceTests() => _basketContext.Setup(b => b.SiteRootCulture).Returns("en-GB");

        private ICoreScopeProvider Scopes()
        {
            var publisher = new Mock<IScopedNotificationPublisher>();
            publisher.Setup(p => p.Publish(It.IsAny<INotification>())).Callback<INotification>(_published.Add);
            var scope = new Mock<ICoreScope> { DefaultValue = DefaultValue.Mock };
            scope.Setup(s => s.Notifications).Returns(publisher.Object);
            var scopes = new Mock<ICoreScopeProvider> { DefaultValue = DefaultValue.Mock };
            scopes.Setup(s => s.CreateCoreScope(It.IsAny<System.Data.IsolationLevel>(), It.IsAny<RepositoryCacheMode>(), It.IsAny<IEventDispatcher?>(), It.IsAny<IScopedNotificationPublisher?>(), It.IsAny<bool?>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(scope.Object);
            return scopes.Object;
        }

        private SessionService Service()
        {
            var httpContext = new DefaultHttpContext { Session = _session };

            // The licence service is not used by the session service.
            return new SessionService(
                new EphemeralDataProtectionProvider(),
                Mock.Of<IHttpContextAccessor>(a => a.HttpContext == httpContext),
                NullLogger<SessionService>.Instance,
                Mock.Of<IEventAggregator>(),
                Scopes(),
                Mock.Of<IConfigurationService>(),
                null!,
                _basketContext.Object);
        }

        private static LineItem Line(string currency) => new() { Key = Guid.NewGuid(), CurrencyCode = currency, Price = 1m, Quantity = 1 };

        [Fact]
        public async Task Get_KeepsASeparateBasketPerCurrency()
        {
            var service = Service();
            var gbp = Line("GBP");
            await service.Update(new Basket { CurrencyCode = "GBP", LineItems = new[] { gbp } });

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
                Basket = new Basket { SiteRootCulture = "en-GB", LineItems = new[] { gbp, usd, Line("") } }
            });
            var service = Service();

            Assert.Equal(gbp.Key, Assert.Single((await service.Get("GBP")).Basket.LineItems).Key);
            Assert.Equal(usd.Key, Assert.Single((await service.Get("USD")).Basket.LineItems).Key);
            Assert.DoesNotContain(BasketKeys.LegacySession("en-GB"), _session.Keys);
        }

        [Fact]
        public async Task Get_ALegacyBasketSplitDoesNotReplaceANewerBasket()
        {
            var current = Line("GBP");
            _session.SetObjectAsJson(BasketKeys.Session("en-GB", "GBP"), new UmbCheckoutSession { SiteRootCulture = "en-GB", Basket = new Basket { CurrencyCode = "GBP", LineItems = new[] { current } } });
            _session.SetObjectAsJson(BasketKeys.LegacySession("en-GB"), new UmbCheckoutSession { Basket = new Basket { LineItems = new[] { Line("GBP") } } });

            Assert.Equal(current.Key, Assert.Single((await Service().Get("GBP")).Basket.LineItems).Key);
        }

        [Fact]
        public async Task Update_StoresTheBasketUnderItsCurrency()
        {
            await Service().Update(new Basket { CurrencyCode = "USD", LineItems = new[] { Line("USD") } });

            Assert.Contains(BasketKeys.Session("en-GB", "USD"), _session.Keys);
        }

        [Fact]
        public async Task Update_ABasketWithoutACurrencyTakesTheRequestsCurrency()
        {
            _basketContext.Setup(b => b.GetCurrencyAsync()).ReturnsAsync("GBP");

            var session = await Service().Update(new Basket { LineItems = new[] { Line("GBP") } });

            Assert.Equal("GBP", session.Basket.CurrencyCode);
        }

        [Fact]
        public async Task Update_DoesNotReadTheBasketFirst()
        {
            // Its inner Get queued OnSessionGetNotification in the same scope: the cookie handler then wrote the request's old
            // cookie basket back over the update in the session.
            await Service().Update(new Basket { CurrencyCode = "GBP", LineItems = new[] { Line("GBP") } });

            Assert.DoesNotContain(_published, n => n is OnSessionGetNotification);
            Assert.Single(_published.OfType<OnSessionUpdatedNotification>());
        }

        [Fact]
        public async Task Clear_RemovesOnlyThatCurrencysBasket()
        {
            var service = Service();
            await service.Update(new Basket { CurrencyCode = "GBP", LineItems = new[] { Line("GBP") } });
            await service.Update(new Basket { CurrencyCode = "USD", LineItems = new[] { Line("USD") } });

            await service.Clear("GBP");

            Assert.Empty((await service.Get("GBP")).Basket.LineItems);
            Assert.Single((await service.Get("USD")).Basket.LineItems);
        }

        [Fact]
        public async Task Clear_KeepsTheRestOfTheHttpSession()
        {
            _session.SetString("AnotherPackagesKey", "kept");

            await Service().Clear("GBP");

            Assert.Equal("kept", _session.GetString("AnotherPackagesKey"));
        }

        [Fact]
        public async Task Clear_UsesTheRequestsCurrencyByDefault()
        {
            _basketContext.Setup(b => b.GetCurrencyAsync()).ReturnsAsync("GBP");
            var service = Service();
            await service.Update(new Basket { CurrencyCode = "GBP", LineItems = new[] { Line("GBP") } });

            await service.Clear();

            Assert.DoesNotContain(BasketKeys.Session("en-GB", "GBP"), _session.Keys);
        }

        [Fact]
        public async Task Clear_WithAnExplicitCultureWorksWhileThePageIsUnknown()
        {
            // The success handler runs while routing: the request's culture is not known yet.
            var service = Service();
            await service.Update(new Basket { CurrencyCode = "GBP", LineItems = new[] { Line("GBP") } });
            _basketContext.Setup(b => b.SiteRootCulture).Returns((string?)null);

            await service.Clear("en-GB", "GBP");

            _basketContext.Setup(b => b.SiteRootCulture).Returns("en-GB");
            Assert.Empty((await service.Get("GBP")).Basket.LineItems);
        }

        [Fact]
        public async Task Clear_APaidBasketSavedBeforeTheUpgradeIsNotSplitBackLater()
        {
            // A checkout started before the upgrade: its basket is still under the culture-only key when the success page clears it.
            var usd = Line("USD");
            _session.SetObjectAsJson(BasketKeys.LegacySession("en-GB"), new UmbCheckoutSession { SiteRootCulture = "en-GB", Basket = new Basket { LineItems = new[] { Line("GBP"), usd } } });
            var service = Service();

            await service.Clear("en-GB", "GBP");

            Assert.Empty((await service.Get("GBP")).Basket.LineItems);
            Assert.Equal(usd.Key, Assert.Single((await service.Get("USD")).Basket.LineItems).Key);
        }

        [Fact]
        public async Task Clear_TellsHandlersWhichBasketWasCleared()
        {
            await Service().Clear("en-GB", "USD");

            var cleared = Assert.Single(_published.OfType<OnSessionClearedNotification>());
            Assert.Equal(("en-GB", "USD"), (cleared.SiteRootCulture, cleared.CurrencyCode));
        }

        [Fact]
        public void DependencyInjection_UsesTheConstructorWithTheBasketContext()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
            services.AddSingleton(Mock.Of<IHttpContextAccessor>());
            services.AddSingleton<ILogger<SessionService>>(NullLogger<SessionService>.Instance);
            services.AddSingleton(Mock.Of<IEventAggregator>());
            services.AddSingleton(Mock.Of<ICoreScopeProvider>());
            services.AddSingleton(Mock.Of<IConfigurationService>());
            services.AddSingleton(Mock.Of<IUmbracoContextAccessor>());
            services.AddSingleton(_basketContext.Object);
            services.AddSingleton(LicenceServiceStandIn());

            // Both public constructors can be satisfied here: the registration must choose the new one, not fail as ambiguous.
            Assert.IsType<SessionService>(RegisterServicesComposer.CreateSessionService(services.BuildServiceProvider()));
        }

        [Fact]
        public void TheOldConstructorIsObsolete()
            => Assert.Single(typeof(SessionService).GetConstructors(), c => c.GetCustomAttributes(typeof(ObsoleteAttribute), false).Any()
                                                                          && c.GetParameters().Last().ParameterType == typeof(IUmbracoContextAccessor));

        /// <summary>The session service never calls its licence service: an uninitialised instance stands in.</summary>
        private static UmbHost.Licensing.Services.LicenseService LicenceServiceStandIn()
            => (UmbHost.Licensing.Services.LicenseService)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(UmbHost.Licensing.Services.LicenseService));
    }
}
