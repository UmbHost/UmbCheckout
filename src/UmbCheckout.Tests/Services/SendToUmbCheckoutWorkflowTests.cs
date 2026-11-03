using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Stripe.Checkout;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Models;
using UmbCheckout.Shared.Notifications;
using UmbCheckout.Stripe.Forms.Workflows;
using UmbCheckout.Stripe.Interfaces;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Core.Models;
using Umbraco.Forms.Core.Persistence.Dtos;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    public class SendToUmbCheckoutWorkflowTests
    {
        [Fact]
        public async Task Forms_checkout_records_the_Stripe_session_on_the_basket_it_pays_for()
        {
            // As StripeBasketController.Checkout: without the session ID neither the success page nor the
            // webhook's paid-basket clearing can tell this basket was paid, so it is never cleared.
            var basket = new Basket { SiteRootCulture = "en-GB", CurrencyCode = "GBP", LineItems = [new LineItem { Key = Guid.NewGuid(), Name = "Ball", Price = 2.5m, Quantity = 1, CurrencyCode = "GBP" }] };
            var basketService = new Mock<IBasketService>();
            basketService.Setup(b => b.Get(It.IsAny<CancellationToken>())).ReturnsAsync(basket);
            var pricing = new Mock<IStripeBasketPricingService>();
            pricing.Setup(p => p.RepriceAsync(basket, It.IsAny<CancellationToken>())).ReturnsAsync(new BasketRepricing(basket, []));
            var stripe = new Mock<IStripeSessionService>();
            stripe.Setup(s => s.CreateSessionAsync(basket, It.IsAny<CancellationToken>())).ReturnsAsync(new Session { Id = "cs_test_forms", Url = "https://checkout.stripe.com/c/pay/cs_test_forms" });
            var sessions = new Mock<ISessionService>();
            Basket? saved = null;
            var suppressed = false;
            CancellationToken updateToken = new(true);
            sessions.Setup(s => s.Update(It.IsAny<Basket>(), It.IsAny<CancellationToken>()))
                .Callback<Basket, CancellationToken>((b, t) => { saved = b; updateToken = t; suppressed = UmbCheckoutHooks.SavingSuppressed; })
                .ReturnsAsync(new UmbCheckoutSession());
            var httpContext = new DefaultHttpContext();

            var workflow = new SendToUmbCheckoutWorkflow(NullLogger<SendToUmbCheckoutWorkflow>.Instance, new HttpContextAccessor { HttpContext = httpContext },
                basketService.Object, stripe.Object, pricing.Object, sessions.Object);

            var context = new WorkflowExecutionContext(new Umbraco.Forms.Core.Persistence.Dtos.Record { UniqueId = Guid.NewGuid() }, new Form { Id = Guid.NewGuid() }, FormState.Submitted, new System.Collections.Hashtable());

            Assert.Equal(WorkflowExecutionStatus.Completed, await workflow.ExecuteAsync(context));
            Assert.Equal("cs_test_forms", saved?.CheckoutSessionId);
            Assert.True(suppressed, "Recording the session is bookkeeping: a site's Saving handlers must not refuse it");
            Assert.False(UmbCheckoutHooks.SavingSuppressed);
            // Not cancelled with the request: Stripe already has the session.
            Assert.False(updateToken.CanBeCanceled);
            Assert.False(updateToken.IsCancellationRequested);
            Assert.Equal("https://checkout.stripe.com/c/pay/cs_test_forms", httpContext.Items[Constants.ItemKeys.RedirectAfterFormSubmitUrl]);
        }
    }
}
