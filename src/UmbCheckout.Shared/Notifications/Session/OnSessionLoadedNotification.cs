using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using UmbCheckout.Shared.Models;

namespace UmbCheckout.Shared.Notifications.Session
{
    /// <summary>
    /// Published after a basket is read, before it is returned; changes to <see cref="Target"/> reach the caller (they are not stored).
    /// What is charged is always re-priced from published content at checkout. Keep handlers cheap: it runs on every read.
    /// </summary>
    public sealed class OnSessionLoadedNotification(UmbCheckoutSession target, EventMessages messages)
        : ObjectNotification<UmbCheckoutSession>(target, messages)
    {
        /// <summary>The session, with its basket, about to be returned.</summary>
        public new UmbCheckoutSession Target => base.Target;
    }
}
