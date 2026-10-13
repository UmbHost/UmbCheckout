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

        private readonly Mock<IEventAggregator> _events = new();
        private readonly Mock<IPaidCheckoutSessions> _paid = new();

        private SessionService Service()
        {
            var httpContext = new DefaultHttpContext { Session = _session };
            _basketContext.Setup(b => b.SiteRootCulture).Returns("en-GB");
            var licenseService = UmbCheckout.Testing.TestLicenseService.Create();

            return new SessionService(
                new EphemeralDataProtectionProvider(),
                Mock.Of<IHttpContextAccessor>(a => a.HttpContext == httpContext),
                NullLogger<SessionService>.Instance,
                _events.Object,
                new Mock<ICoreScopeProvider> { DefaultValue = DefaultValue.Mock }.Object,
                Mock.Of<IConfigurationService>(),
                licenseService,
                _basketContext.Object,
                new UmbCheckout.Testing.TestEventMessagesFactory(),
                _paid.Object);
        }

        // Sub-project B: a customer who paid and never came back to the success page keeps the paid basket; the webhook records
        // the paid checkout session, and the next basket read clears it.
        private async Task<SessionService> WithCheckedOutBasket(string checkoutSessionId)
        {
            var service = Service();
            await service.Update(new Basket { CurrencyCode = "GBP", LineItems = [Line("GBP")], CheckoutSessionId = checkoutSessionId });
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
            Assert.Empty(_session.GetObjectFromJson<UmbCheckoutSession>(BasketKeys.Session("en-GB", "GBP"))?.Basket.LineItems ?? []);
            _events.Verify(e => e.PublishAsync(It.Is<UmbCheckout.Shared.Notifications.Session.OnPaidBasketClearedNotification>(n =>
                n.CheckoutSessionId == "cs_test_paid" && n.Basket.LineItems.Count() == 1), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task A_basket_whose_checkout_session_is_not_paid_stays()
        {
            var service = await WithCheckedOutBasket("cs_test_open");

            Assert.Single((await service.Get("GBP")).Basket.LineItems);
        }

        [Fact]
        public async Task A_basket_that_never_reached_checkout_makes_no_lookup()
        {
            var service = Service();
            await service.Update(new Basket { CurrencyCode = "GBP", LineItems = [Line("GBP")] });

            await service.Get("GBP");

            _paid.Verify(p => p.IsPaidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task An_unpaid_checkout_session_is_looked_up_once_per_request()
        {
            // Review: every basket read (page, partials) of a basket left at checkout made a database lookup.
            var service = await WithCheckedOutBasket("cs_test_open");

            await service.Get("GBP");
            await service.Get("GBP");
            await service.Get("GBP");

            _paid.Verify(p => p.IsPaidAsync("cs_test_open", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Changing_the_lines_after_checkout_forgets_the_checkout_session()
        {
            // Review: a delayed payment (Bacs, SEPA) recorded days later cleared lines added after checkout. A changed basket isn't
            // the one that was paid for, so neither the webhook nor the success page clears it.
            var service = await WithCheckedOutBasket("cs_test_delayed");
            var basket = (await service.Get("GBP")).Basket;

            await service.Update(new Basket { CurrencyCode = "GBP", LineItems = [.. basket.LineItems, Line("GBP")], CheckoutSessionId = basket.CheckoutSessionId });

            Assert.Null((await service.Get("GBP")).Basket.CheckoutSessionId);
        }

        [Fact]
        public async Task Recording_the_checkout_session_on_an_unchanged_basket_keeps_it()
        {
            var service = Service();
            var line = Line("GBP");
            await service.Update(new Basket { CurrencyCode = "GBP", LineItems = [line] });

            await service.Update(new Basket { CurrencyCode = "GBP", LineItems = [line], CheckoutSessionId = "cs_test_new" });

            Assert.Equal("cs_test_new", (await service.Get("GBP")).Basket.CheckoutSessionId);
        }

        [Fact]
        public async Task A_failed_lookup_keeps_the_basket_and_does_not_fail_the_read()
        {
            _paid.Setup(p => p.IsPaidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("database down"));
            var service = await WithCheckedOutBasket("cs_test_paid");

            Assert.Single((await service.Get("GBP")).Basket.LineItems);
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
            _basketContext.Setup(b => b.GetCurrencyAsync(It.IsAny<CancellationToken>())).ReturnsAsync("EUR");

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

        [Fact]
        public async Task Clear_WithAnExplicitCultureWorksWhileThePageIsUnknown()
        {
            // The success handler runs while routing: the request's culture is not known yet.
            var service = Service();
            await service.Update(new Basket { CurrencyCode = "GBP", LineItems = [Line("GBP")] });
            _basketContext.Setup(b => b.SiteRootCulture).Returns((string?)null);

            await service.Clear("en-GB", "GBP");

            _basketContext.Setup(b => b.SiteRootCulture).Returns("en-GB");
            Assert.Empty((await service.Get("GBP")).Basket.LineItems);
        }
    }
}
