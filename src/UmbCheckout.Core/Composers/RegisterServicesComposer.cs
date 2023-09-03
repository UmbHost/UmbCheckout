using Microsoft.Extensions.DependencyInjection;
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
            builder.Services.AddTransient<ISessionService, SessionService>();
            builder.Services.AddTransient<IBasketService, BasketService>();
            builder.Services.AddTransient<IConfigurationService, ConfigurationService>();
            builder.Services.AddTransient<IDatabaseService, DatabaseService>();
        }
    }
}
