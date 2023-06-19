using Microsoft.Extensions.DependencyInjection;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Services;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace UmbCheckout.Core.Composers
{
    /// <summary>
    /// Registers the required services
    /// </summary>
    public class RegisterServicesComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddTransient<ISessionService, SessionService>();
            builder.Services.AddTransient<IBasketService, BasketService>();
            builder.Services.AddTransient<IConfigurationService, ConfigurationService>();
            builder.Services.AddTransient<IDatabaseService, DatabaseService>();
        }
    }
}
