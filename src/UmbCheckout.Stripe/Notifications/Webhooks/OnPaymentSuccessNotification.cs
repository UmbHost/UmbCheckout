using Stripe;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Stripe.Notifications.Webhooks
{
    public class OnPaymentSuccessNotification : INotification
    {
        public Event? StripeEvent { get; set; }

        public OnPaymentSuccessNotification(Event? stripeEvent)
        {
            StripeEvent = stripeEvent;
        }
    }
}
