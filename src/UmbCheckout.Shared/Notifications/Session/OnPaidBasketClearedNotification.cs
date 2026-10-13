using Umbraco.Cms.Core.Notifications;
using UmbCheckout.Shared.Models;

namespace UmbCheckout.Shared.Notifications.Session
{
    /// <summary>
    /// Published when a basket read finds the basket's checkout session paid (recorded by the payment provider's webhook) and
    /// clears it: the customer paid but never returned to the success page. <see cref="Basket"/> is the basket as it was.
    /// </summary>
    public sealed class OnPaidBasketClearedNotification(Models.Basket basket, string checkoutSessionId) : INotification
    {
        /// <summary>The paid basket that was cleared.</summary>
        public Models.Basket Basket { get; } = basket;

        /// <summary>The payment provider's checkout session the basket was paid with.</summary>
        public string CheckoutSessionId { get; } = checkoutSessionId;
    }
}
