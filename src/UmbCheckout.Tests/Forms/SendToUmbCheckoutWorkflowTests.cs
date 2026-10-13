using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Forms.Workflows;
using UmbCheckout.Stripe.Interfaces;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Core.Models;
using Xunit;
using Session = Stripe.Checkout.Session;

namespace UmbCheckout.Tests.Forms
{
    public class SendToUmbCheckoutWorkflowTests
    {
        [Fact]
        public void Forms_checkout_records_the_Stripe_session_on_the_basket_it_pays_for()
        {
            // As StripeBasketController.Checkout: without the session id the success page can't tell this basket was paid.
            var basket = new Basket { SiteRootCulture = "en-GB", CurrencyCode = "GBP", LineItems = new[] { new LineItem { Key = Guid.NewGuid(), Name = "Ball", Price = 2.5m, Quantity = 1, CurrencyCode = "GBP" } } };
            var basketService = new Mock<IBasketService>();
            basketService.Setup(b => b.Get()).ReturnsAsync(basket);
            var pricing = new Mock<IStripeBasketPricingService>();
            pricing.Setup(p => p.RepriceAsync(basket)).ReturnsAsync(new BasketRepricing(basket, Array.Empty<RepricedLine>()));
            var stripe = new Mock<IStripeSessionService>();
            stripe.Setup(s => s.CreateSessionAsync(basket)).ReturnsAsync(new Session { Id = "cs_test_forms", Url = "https://checkout.stripe.com/c/pay/cs_test_forms" });
            var sessions = new Mock<ISessionService>();
            Basket? saved = null;
            sessions.Setup(s => s.Update(It.IsAny<Basket>())).Callback<Basket>(b => saved = b).ReturnsAsync(new UmbCheckoutSession());
            var httpContext = new DefaultHttpContext();

            var workflow = new SendToUmbCheckoutWorkflow(NullLogger<SendToUmbCheckoutWorkflow>.Instance, new HttpContextAccessor { HttpContext = httpContext },
                basketService.Object, stripe.Object, pricing.Object, sessions.Object);
            var context = new WorkflowExecutionContext(new Umbraco.Forms.Core.Persistence.Dtos.Record { UniqueId = Guid.NewGuid() }, new Form { Id = Guid.NewGuid() }, FormState.Submitted, new System.Collections.Hashtable());

#if NET8_0_OR_GREATER
            var status = workflow.ExecuteAsync(context).GetAwaiter().GetResult();
#else
            var status = workflow.Execute(context);
#endif

            Assert.Equal(WorkflowExecutionStatus.Completed, status);
            Assert.Equal("cs_test_forms", saved?.CheckoutSessionId);
            Assert.Equal("https://checkout.stripe.com/c/pay/cs_test_forms", httpContext.Items[Constants.ItemKeys.RedirectAfterFormSubmitUrl]);
        }

        [Fact]
        public void The_older_constructors_are_obsolete_and_DI_picks_the_one_with_the_session_service()
        {
            var constructors = typeof(SendToUmbCheckoutWorkflow).GetConstructors();

            Assert.Equal(2, constructors.Count(c => c.GetCustomAttributes(typeof(ObsoleteAttribute), false).Any()));
            Assert.Contains(typeof(ISessionService), constructors.Single(c => c.GetCustomAttributes(typeof(Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructorAttribute), false).Any()).GetParameters().Select(p => p.ParameterType));
        }
    }
}
