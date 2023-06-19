using Microsoft.Extensions.DependencyInjection;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Services;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace UmbCheckout.Stripe.Composers
{
    public class RegisterServicesComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddTransient<IStripeSessionService, StripeSessionService>();
            builder.Services.AddTransient<IStripeShippingRateDatabaseService, StripeShippingRateDatabaseService>();
        }
    }
}
