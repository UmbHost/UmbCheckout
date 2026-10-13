#pragma warning disable CS0618 // these tests cover the overloads without a token, which are obsolete but kept
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Session = Stripe.Checkout.Session;
using SessionService = UmbCheckout.Core.Services.SessionService;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Services;
using UmbCheckout.Shared.Helpers;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.NotificationHandlers;
using UmbCheckout.Tests.Infrastructure;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    /// <summary>
    /// A checkout started before the upgrade returns to the success page after it: the Stripe session has no basket metadata and
    /// the basket is still under the culture-only legacy key. The handler runs while routing, when the basket context has no
    /// culture (it is null), so the culture comes from the routed page. A real SessionService, not a mock.
    /// </summary>
    public class SuccessPageLegacyBasketTests
    {
        private readonly InMemorySession _session = new();
        private readonly Mock<IBasketContext> _basketContext = new();

        private SessionService Sessions()
        {
            var httpContext = new DefaultHttpContext { Session = _session };
            var scopes = new Mock<ICoreScopeProvider> { DefaultValue = DefaultValue.Mock };
            return new SessionService(new EphemeralDataProtectionProvider(), Mock.Of<IHttpContextAccessor>(a => a.HttpContext == httpContext),
                NullLogger<SessionService>.Instance, Mock.Of<IEventAggregator>(), scopes.Object, Mock.Of<IConfigurationService>(), null!, _basketContext.Object);
        }

        private static LineItem Line(string currency) => new() { Key = Guid.NewGuid(), CurrencyCode = currency, Price = 1m, Quantity = 1 };

        [Fact]
        public async Task A_pre_upgrade_checkout_clears_its_legacy_basket_on_the_success_page()
        {
            var usd = Line("USD");
            _session.SetObjectAsJson(BasketKeys.LegacySession("en-GB"), new UmbCheckoutSession { SiteRootCulture = "en-GB", Basket = new Basket { SiteRootCulture = "en-GB", LineItems = new[] { Line("GBP"), usd } } });
            _basketContext.Setup(b => b.SiteRootCulture).Returns((string?)null); // while routing

            var stripe = new Mock<IStripeSessionService>();
            stripe.Setup(s => s.GetSessionAsync("cs_test_1", It.IsAny<CancellationToken>())).ReturnsAsync(new Session { Id = "cs_test_1", Status = "complete", Currency = "gbp", Created = DateTime.UtcNow.AddMinutes(-5), Metadata = new Dictionary<string, string>() });
            var page = Mock.Of<IPublishedContent>(p => p.Key == Guid.NewGuid());
            var request = new Mock<IPublishedRequestBuilder>();
            request.Setup(r => r.Uri).Returns(new Uri("https://shop.test/success/?session_id=cs_test_1&success=true"));
            request.Setup(r => r.PublishedContent).Returns(page);

            await SuccessHandler.Create(stripe.Object, Sessions(), _ => "en-GB").HandleAsync(new RoutingRequestNotification(request.Object), CancellationToken.None);

            _basketContext.Setup(b => b.SiteRootCulture).Returns("en-GB"); // the page renders
            var sessions = Sessions();
            Assert.Empty((await sessions.Get("GBP")).Basket.LineItems);
            Assert.Equal(usd.Key, Assert.Single((await sessions.Get("USD")).Basket.LineItems).Key);
        }
    }
}
