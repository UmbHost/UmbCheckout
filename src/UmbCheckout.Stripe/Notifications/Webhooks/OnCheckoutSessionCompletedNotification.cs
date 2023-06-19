using Stripe;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Stripe.Notifications.Webhooks
{
    public class OnCheckoutSessionCompletedNotification : INotification
    {
        public Event? StripeEvent { get; set; }

        public OnCheckoutSessionCompletedNotification(Event? stripeEvent)
        {
            StripeEvent = stripeEvent;
        }
    }
}
