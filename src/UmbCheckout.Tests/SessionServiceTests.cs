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
        private readonly Mock<IEventAggregator> _events = new();
        private readonly Mock<IPaidCheckoutSessions> _paid = new();
        private readonly DefaultHttpContext _httpContext;

        public SessionServiceTests()
        {
            _basketContext.Setup(b => b.SiteRootCulture).Returns("en-GB");
            _httpContext = new DefaultHttpContext { Session = _session };
        }

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

        private SessionService Service(ILogger<SessionService>? logger = null)
        {
            // The licence service is not used by the session service.
            return new SessionService(
                new EphemeralDataProtectionProvider(),
                Mock.Of<IHttpContextAccessor>(a => a.HttpContext == _httpContext),
                logger ?? NullLogger<SessionService>.Instance,
                _events.Object,
                Scopes(),
                Mock.Of<IConfigurationService>(),
                null!,
                _basketContext.Object,
                _paid.Object);
        }

        // Group 5 (v17 sub-project B): a customer who paid and never came back to the success page keeps the paid basket; the
        // webhook records the paid checkout session, and the next basket read clears it.
        private async Task<SessionService> WithCheckedOutBasket(string checkoutSessionId)
        {
            var service = Service();
            await service.Update(new Basket { CurrencyCode = "GBP", LineItems = new[] { Line("GBP") }, CheckoutSessionId = checkoutSessionId });
            return service;
        }

        [Fact]
        public async Task A_basket_whose_checkout_session_was_paid_is_cleared_on_the_next_read()
        {
            _paid.Setup(p => p.IsPaidAsync("cs_test_paid", It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var service = await WithCheckedOutBasket("cs_test_paid");

            var session = await service.Get("GBP");

            Assert.Empty(session.Basket.LineItems);
            Assert.Null(session.Basket.CheckoutSessionId);
            Assert.Equal("GBP", session.Basket.CurrencyCode);
            Assert.Empty(_session.GetObjectFromJson<UmbCheckoutSession>(BasketKeys.Session("en-GB", "GBP"))?.Basket.LineItems ?? Enumerable.Empty<LineItem>());
            var cleared = Assert.Single(_published.OfType<OnSessionClearedNotification>());
            Assert.Equal(("en-GB", "GBP"), (cleared.SiteRootCulture, cleared.CurrencyCode));
            _events.Verify(e => e.PublishAsync(It.Is<OnPaidBasketClearedNotification>(n =>
                n.CheckoutSessionId == "cs_test_paid" && n.Basket.LineItems.Count() == 1), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task The_paid_basket_is_cleared_once()
        {
            _paid.Setup(p => p.IsPaidAsync("cs_test_paid", It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var service = await WithCheckedOutBasket("cs_test_paid");

            await service.Get("GBP");
            Assert.Empty((await service.Get("GBP")).Basket.LineItems);

            _events.Verify(e => e.PublishAsync(It.IsAny<OnPaidBasketClearedNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task A_basket_whose_checkout_session_is_not_paid_stays()
        {
            var service = await WithCheckedOutBasket("cs_test_open");

            Assert.Single((await service.Get("GBP")).Basket.LineItems);
            Assert.Empty(_published.OfType<OnSessionClearedNotification>());
        }

        [Fact]
        public async Task A_basket_that_never_reached_checkout_makes_no_lookup()
        {
            var service = Service();
            await service.Update(new Basket { CurrencyCode = "GBP", LineItems = new[] { Line("GBP") } });

            await service.Get("GBP");

            _paid.Verify(p => p.IsPaidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task An_unpaid_checkout_session_is_looked_up_once_per_request()
        {
            // v17 review: every basket read (page, partials) of a basket left at checkout made a database lookup.
            var service = await WithCheckedOutBasket("cs_test_open");

            await service.Get("GBP");
            await service.Get("GBP");
            await Service().Get("GBP");

            _paid.Verify(p => p.IsPaidAsync("cs_test_open", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Changing_the_lines_after_checkout_forgets_the_checkout_session()
        {
            // v17 review: a delayed payment (Bacs, SEPA) recorded days later cleared lines added after checkout. A changed basket
            // is not the one that was paid for, so neither the webhook nor the success page clears it.
            var service = await WithCheckedOutBasket("cs_test_delayed");
            var basket = (await service.Get("GBP")).Basket;

            await service.Update(new Basket { CurrencyCode = "GBP", LineItems = basket.LineItems.Append(Line("GBP")).ToArray(), CheckoutSessionId = basket.CheckoutSessionId });

            Assert.Null((await service.Get("GBP")).Basket.CheckoutSessionId);
        }

        [Fact]
        public async Task Changing_a_quantity_after_checkout_forgets_the_checkout_session()
        {
            var service = await WithCheckedOutBasket("cs_test_delayed");
            var line = Assert.Single((await service.Get("GBP")).Basket.LineItems);
            line.Quantity = 5;

            await service.Update(new Basket { CurrencyCode = "GBP", LineItems = new[] { line }, CheckoutSessionId = "cs_test_delayed" });

            Assert.Null((await service.Get("GBP")).Basket.CheckoutSessionId);
        }

        [Fact]
        public async Task Recording_the_checkout_session_on_an_unchanged_basket_keeps_it()
        {
            var service = Service();
            var line = Line("GBP");
            await service.Update(new Basket { CurrencyCode = "GBP", LineItems = new[] { line } });

            await service.Update(new Basket { CurrencyCode = "GBP", LineItems = new[] { line }, CheckoutSessionId = "cs_test_new" });

            Assert.Equal("cs_test_new", (await service.Get("GBP")).Basket.CheckoutSessionId);
        }

        [Fact]
        public async Task A_failed_lookup_keeps_the_basket_and_does_not_fail_the_read()
        {
            _paid.Setup(p => p.IsPaidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("database down"));
            var logger = new UmbCheckout.Tests.Helpers.RecordingLogger<SessionService>();
            await WithCheckedOutBasket("cs_test_paid");

            Assert.Single((await Service(logger).Get("GBP")).Basket.LineItems);
            Assert.Equal(1, logger.Count(LogLevel.Warning));
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
            _basketContext.Setup(b => b.GetCurrencyAsync(It.IsAny<CancellationToken>())).ReturnsAsync("EUR");

            Assert.Equal("EUR", (await Service().Get()).Basket.CurrencyCode);
        }

        [Fact]
        public async Task A_cancelled_update_stores_nothing()
        {
            // A client disconnect stops the update before the basket is written, never part-way (session, cookie and database copies).
            var service = Service();
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.Update(new Basket { CurrencyCode = "GBP", LineItems = new[] { Line("GBP") } }, cancelled.Token));

            Assert.Empty(_session.Keys);
            Assert.DoesNotContain(_published, n => n is OnSessionUpdatedNotification);
        }

        [Fact]
        public async Task The_overloads_without_a_token_use_the_requests()
        {
            using var request = new CancellationTokenSource();
            _httpContext.RequestAborted = request.Token;
            _basketContext.Setup(b => b.GetCurrencyAsync(It.IsAny<CancellationToken>())).ReturnsAsync("EUR");

            await Service().Get();
            await Service().Update(new Basket { LineItems = new[] { Line("EUR") } });

            _basketContext.Verify(b => b.GetCurrencyAsync(request.Token), Times.Exactly(2));
            _events.Verify(e => e.PublishAsync(It.IsAny<OnSessionGetStartedNotification>(), request.Token), Times.Once);
            _events.Verify(e => e.PublishAsync(It.IsAny<OnSessionUpdateStartedNotification>(), request.Token), Times.Once);
        }

        [Fact]
        public async Task Get_WithAnExplicitCultureWorksWhileThePageIsUnknown()
        {
            // While routing the basket context has no page (culture null); the success page names the basket's culture.
            _session.SetObjectAsJson(BasketKeys.LegacySession("en-GB"), new UmbCheckoutSession { SiteRootCulture = "en-GB", Basket = new Basket { LineItems = new[] { Line("GBP") } } });
            _basketContext.Setup(b => b.SiteRootCulture).Returns((string?)null);

            var session = await Service().Get("en-GB", "GBP");

            Assert.Single(session.Basket.LineItems);
            Assert.Equal("en-GB", session.SiteRootCulture);
        }

        [Fact]
        public async Task The_checkout_session_is_kept_with_the_basket()
        {
            var service = Service();
            await service.Update(new Basket { CurrencyCode = "GBP", CheckoutSessionId = "cs_test_1", LineItems = new[] { Line("GBP") } });

            Assert.Equal("cs_test_1", (await service.Get("GBP")).Basket.CheckoutSessionId);
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
            _basketContext.Setup(b => b.GetCurrencyAsync(It.IsAny<CancellationToken>())).ReturnsAsync("GBP");

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
            _basketContext.Setup(b => b.GetCurrencyAsync(It.IsAny<CancellationToken>())).ReturnsAsync("GBP");
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
            services.AddSingleton<IPaidCheckoutSessions, NoPaidCheckoutSessions>();

            // Both public constructors can be satisfied here: the registration must choose the new one, not fail as ambiguous.
            Assert.IsType<SessionService>(RegisterServicesComposer.CreateSessionService(services.BuildServiceProvider()));
        }

        [Fact]
        public void TheOldConstructorIsObsolete()
            => Assert.Single(typeof(SessionService).GetConstructors(), c => c.GetCustomAttributes(typeof(ObsoleteAttribute), false).Any()
                                                                          && c.GetParameters().Last().ParameterType == typeof(IUmbracoContextAccessor));

        [Fact]
        public void The_constructor_without_the_paid_session_store_is_kept_obsolete_and_DI_picks_the_new_one()
        {
            var constructors = typeof(SessionService).GetConstructors();

            Assert.Single(constructors, c => c.GetCustomAttributes(typeof(ObsoleteAttribute), false).Any()
                                             && c.GetParameters().Last().ParameterType == typeof(IBasketContext));
            var current = Assert.Single(constructors, c => c.GetCustomAttributes(typeof(ActivatorUtilitiesConstructorAttribute), false).Any());
            Assert.Equal(typeof(IPaidCheckoutSessions), current.GetParameters().Last().ParameterType);
            Assert.Empty(current.GetCustomAttributes(typeof(ObsoleteAttribute), false));
        }

        [Fact]
        public async Task The_obsolete_constructor_without_a_container_asks_no_store()
        {
#pragma warning disable CS0618 // the obsolete constructor is the subject
            var service = new SessionService(new EphemeralDataProtectionProvider(), Mock.Of<IHttpContextAccessor>(a => a.HttpContext == _httpContext),
                NullLogger<SessionService>.Instance, _events.Object, Scopes(), Mock.Of<IConfigurationService>(), null!, _basketContext.Object);
#pragma warning restore CS0618
            await service.Update(new Basket { CurrencyCode = "GBP", LineItems = new[] { Line("GBP") }, CheckoutSessionId = "cs_test_paid" });

            Assert.Single((await service.Get("GBP")).Basket.LineItems);
        }

        /// <summary>The session service never calls its licence service: an uninitialised instance stands in.</summary>
        private static UmbHost.Licensing.Services.LicenseService LicenceServiceStandIn()
            => (UmbHost.Licensing.Services.LicenseService)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(UmbHost.Licensing.Services.LicenseService));
    }
}
