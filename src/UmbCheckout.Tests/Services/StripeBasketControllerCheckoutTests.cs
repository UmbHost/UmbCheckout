using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Stripe.Checkout;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Controllers.Surface;
using UmbCheckout.Stripe.Interfaces;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    public class StripeBasketControllerCheckoutTests
    {
        [Fact]
        public async Task Checkout_records_the_Stripe_session_on_the_basket_it_pays_for()
        {
            // The success page clears a basket only when it is the one this checkout session paid for.
            var basket = new Basket { SiteRootCulture = "en-GB", CurrencyCode = "GBP", LineItems = [new LineItem { Key = Guid.NewGuid(), Name = "Ball", Price = 2.5m, Quantity = 1, CurrencyCode = "GBP" }] };
            var basketService = new Mock<IBasketService>();
            basketService.Setup(b => b.Get(It.IsAny<CancellationToken>())).ReturnsAsync(basket);
            var pricing = new Mock<IStripeBasketPricingService>();
            pricing.Setup(p => p.RepriceAsync(basket, It.IsAny<CancellationToken>())).ReturnsAsync(new BasketRepricing(basket, []));
            var stripe = new Mock<IStripeSessionService>();
            stripe.Setup(s => s.CreateSessionAsync(basket, It.IsAny<CancellationToken>())).ReturnsAsync(new Session { Id = "cs_test_9", Url = "https://checkout.stripe.com/c/pay/cs_test_9" });
            var sessions = new Mock<ISessionService>();
            Basket? saved = null;
            sessions.Setup(s => s.Update(It.IsAny<Basket>(), It.IsAny<CancellationToken>())).Callback<Basket, CancellationToken>((b, _) => saved = b).ReturnsAsync(new UmbCheckoutSession());

            var controller = new StripeBasketController(Mock.Of<IUmbracoContextAccessor>(), Mock.Of<IUmbracoDatabaseFactory>(), ServiceContext.CreatePartial(), AppCaches.NoCache,
                Mock.Of<IProfilingLogger>(), Mock.Of<IPublishedUrlProvider>(), basketService.Object, stripe.Object, Mock.Of<IStripeLineItemPricingService>(), pricing.Object,
                sessions.Object, NullLogger<StripeBasketController>.Instance);

            Assert.IsType<RedirectResult>(await controller.Checkout());
            Assert.Equal("cs_test_9", saved?.CheckoutSessionId);
        }

        [Fact]
        public async Task Checkout_passes_the_request_token_to_every_service()
        {
            // A shopper who leaves mid-checkout stops the basket read, the re-pricing and the Stripe call.
            var basket = new Basket { SiteRootCulture = "en-GB", CurrencyCode = "GBP", LineItems = [new LineItem { Key = Guid.NewGuid(), Name = "Ball", Price = 2.5m, Quantity = 1, CurrencyCode = "GBP" }] };
            using var aborted = new CancellationTokenSource();
            var token = aborted.Token;
            var basketService = new Mock<IBasketService>();
            basketService.Setup(b => b.Get(token)).ReturnsAsync(basket);
            var pricing = new Mock<IStripeBasketPricingService>();
            pricing.Setup(p => p.RepriceAsync(basket, token)).ReturnsAsync(new BasketRepricing(basket, []));
            var stripe = new Mock<IStripeSessionService>();
            stripe.Setup(s => s.CreateSessionAsync(basket, token)).ReturnsAsync(new Session { Id = "cs_test_9", Url = "https://checkout.stripe.com/c/pay/cs_test_9" });
            var sessions = new Mock<ISessionService>();
            sessions.Setup(s => s.Update(basket, token)).ReturnsAsync(new UmbCheckoutSession());

            var controller = new StripeBasketController(Mock.Of<IUmbracoContextAccessor>(), Mock.Of<IUmbracoDatabaseFactory>(), ServiceContext.CreatePartial(), AppCaches.NoCache,
                Mock.Of<IProfilingLogger>(), Mock.Of<IPublishedUrlProvider>(), basketService.Object, stripe.Object, Mock.Of<IStripeLineItemPricingService>(), pricing.Object,
                sessions.Object, NullLogger<StripeBasketController>.Instance)
            {
                ControllerContext = new ControllerContext { HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { RequestAborted = token } }
            };

            Assert.IsType<RedirectResult>(await controller.Checkout());
            sessions.Verify(s => s.Update(basket, token), Times.Once);
        }
    }
}
