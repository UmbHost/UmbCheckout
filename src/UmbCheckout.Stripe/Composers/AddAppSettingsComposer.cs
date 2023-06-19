using Microsoft.Extensions.DependencyInjection;
using UmbCheckout.Shared;
using UmbCheckout.Stripe.Models;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace UmbCheckout.Stripe.Composers
{
    public class AddAppSettingsComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.Configure<StripeSettings>(builder.Config.GetSection(Consts.PackageName).GetSection("Stripe"));
        }
    }
}
