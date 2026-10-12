using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using UmbCheckout.Shared.Models;

namespace UmbCheckout.Shared.Notifications.Configuration
{
    /// <summary>
    /// Published after the configuration is read, before it is returned; changes to <see cref="Target"/> reach the caller.
    /// Keep handlers cheap: it runs on every read.
    /// </summary>
    public sealed class OnConfigurationLoadedNotification(UmbCheckoutConfiguration target, EventMessages messages)
        : ObjectNotification<UmbCheckoutConfiguration>(target, messages)
    {
        /// <summary>The configuration about to be returned.</summary>
        public new UmbCheckoutConfiguration Target => base.Target;
    }
}
