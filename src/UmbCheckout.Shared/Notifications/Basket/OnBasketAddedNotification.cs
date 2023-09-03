using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket
{
    /// <summary>
    /// Notification which is triggered after a single line item is added to the Basket
    /// </summary>
    public class OnBasketAddedNotification : INotification
    {
        public LineItem LineItem { get; }
        public Models.Basket Basket { get; set; }

        public OnBasketAddedNotification(LineItem lineItem, Models.Basket basket)
        {
            LineItem = lineItem;
            Basket = basket;
        }
    }
}