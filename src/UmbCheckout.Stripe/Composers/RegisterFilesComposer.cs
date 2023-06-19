using UmbCheckout.Stripe.Files;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace UmbCheckout.Stripe.Composers
{
    public class RegisterFilesComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.BackOfficeAssets()
                .Append<UmbCheckoutShippingRatesJsFIle>();
            builder.BackOfficeAssets()
                .Append<UmbCheckoutShippingRateJsFIle>();
        }
    }
}
