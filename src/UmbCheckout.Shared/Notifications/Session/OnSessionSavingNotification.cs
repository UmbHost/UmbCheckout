using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Session
{
    /// <summary>
    /// Published before a basket change is stored. Change <see cref="Target"/> to change what is stored, or cancel with
    /// <c>CancelOperation(new EventMessage(category, message, EventMessageType.Error))</c>: the basket stays as it was and the
    /// message is available to the page.
    /// </summary>
    public sealed class OnSessionSavingNotification(Models.Basket target, Models.Basket? existing, EventMessages messages)
        : CancelableObjectNotification<Models.Basket>(target, messages)
    {
        /// <summary>The basket about to be stored.</summary>
        public new Models.Basket Target => base.Target;

        /// <summary>The stored basket, or null when there is none yet. For comparison; changes are ignored.</summary>
        public Models.Basket? Existing { get; } = existing;
    }
}
