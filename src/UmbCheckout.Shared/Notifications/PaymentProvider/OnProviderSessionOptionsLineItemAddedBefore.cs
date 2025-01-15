using Stripe.Checkout;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Stripe.Services
{
    /// <summary>
    ///     Notification which is triggered just before a stripe line item is added to the
    ///     line item collection that is sent to Stripe
    /// </summary>
    public class OnProviderSessionOptionsLineItemAddedBefore : INotification
    {
        public SessionLineItemOptions LineItem { get; set; }

        public OnProviderSessionOptionsLineItemAddedBefore(SessionLineItemOptions lineItem)
        {
            LineItem = lineItem;
        }
    }
}