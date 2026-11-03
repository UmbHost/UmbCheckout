using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.NotificationHandlers;
using UmbCheckout.Core.Services;
using UmbCheckout.Shared.Models;
using UmbCheckout.Shared.Notifications.Configuration;
using UmbHost.Licensing.Notifications;
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
            builder.Services.Configure<UmbCheckoutAppSettings>(builder.Config.GetSection("UmbCheckout"));
            builder
                .AddNotificationAsyncHandler<OnConfigurationSavedNotification,
                    UmbCheckoutTelemetryNotificationHandler>();
            builder
                .AddNotificationAsyncHandler<OnLicenseCheckCompletedNotification,
                    UmbCheckoutTelemetryNotificationHandler>();
            builder.Services.AddHttpClient(UmbCheckoutTelemetryNotificationHandler.HttpClientName, UmbCheckoutTelemetryNotificationHandler.ConfigureClient);
            builder.Services.AddTransient<ISessionService, SessionService>();
            // A payment provider (UmbCheckout.Stripe) replaces this with the sessions its webhook recorded as paid.
            builder.Services.TryAddSingleton<IPaidCheckoutSessions, NoPaidCheckoutSessions>();
            builder.Services.AddTransient<IBasketService, BasketService>();
            builder.Services.AddTransient<IConfigurationService, ConfigurationService>();
            builder.Services.AddTransient<ICurrencyService, CurrencyService>();
            builder.Services.AddTransient<IBasketContext, UmbracoBasketContext>();
        }
    }
}
