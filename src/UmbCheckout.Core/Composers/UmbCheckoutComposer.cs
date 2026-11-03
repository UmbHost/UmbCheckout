using Microsoft.Extensions.DependencyInjection;
using UmbCheckout.Core.Extensions;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace UmbCheckout.Core.Composers
{
    public class UmbCheckoutComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = "UmbCheckoutSession";
                options.IdleTimeout = TimeSpan.FromHours(24);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Sites don't need app.UseSession() in Program.cs: the session middleware joins Umbraco's pipeline here, before
            // routing, because the success handler clears the paid basket while Umbraco routes the request.
            builder.Services.Configure<UmbracoPipelineOptions>(options => options.AddFilter(new UmbracoPipelineFilter("UmbCheckout")
            {
                PreRouting = app => app.UseUmbCheckoutSession(),
            }));
        }
    }
}
