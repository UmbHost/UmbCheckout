using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket
{
    /// <summary>
    /// Notification which is triggered before a single line item is added to the Basket
    /// </summary>
    public class OnBasketAddStartedNotification : INotification
    {
        public Models.Basket Basket { get; }
        public LineItem LineItem { get; }

        public OnBasketAddStartedNotification(LineItem lineItem, Models.Basket basket)
        {
            LineItem = lineItem;
            Basket = basket;
        }
    }
}
