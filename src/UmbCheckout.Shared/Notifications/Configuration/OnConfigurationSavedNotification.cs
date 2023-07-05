using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Configuration
{
    /// <summary>
    /// Notification which is triggered after multiple line items are added to the Basket
    /// </summary>
    public class OnConfigurationSavedNotification : INotification
    {
        public UmbCheckoutConfiguration Configuration { get; }

        public OnConfigurationSavedNotification(UmbCheckoutConfiguration configuration)
        {
            Configuration = configuration;
        }
    }
}
