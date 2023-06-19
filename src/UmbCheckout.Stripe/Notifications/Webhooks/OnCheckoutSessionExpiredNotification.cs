using Stripe;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Stripe.Notifications.Webhooks
{
    public class OnCheckoutSessionExpiredNotification : INotification
    {
        public Event? StripeEvent { get; set; }

        public OnCheckoutSessionExpiredNotification(Event? stripeEvent)
        {
            StripeEvent = stripeEvent;
        }
    }
}
