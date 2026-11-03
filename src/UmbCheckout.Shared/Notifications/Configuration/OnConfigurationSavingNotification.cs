using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using UmbCheckout.Shared.Models;

namespace UmbCheckout.Shared.Notifications.Configuration
{
    /// <summary>
    /// Published before the configuration is saved. Change <see cref="Target"/> to change what is saved, or cancel with
    /// <c>CancelOperation(new EventMessage(category, message, EventMessageType.Error))</c>; the message is shown to the person saving.
    /// </summary>
    public sealed class OnConfigurationSavingNotification(UmbCheckoutConfiguration target, UmbCheckoutConfiguration? existing, EventMessages messages)
        : CancelableObjectNotification<UmbCheckoutConfiguration>(target, messages)
    {
        /// <summary>The configuration about to be saved.</summary>
        public new UmbCheckoutConfiguration Target => base.Target;

        /// <summary>The stored configuration, or null when none has been saved yet. For comparison; changes are ignored.</summary>
        public UmbCheckoutConfiguration? Existing { get; } = existing;
    }
}
