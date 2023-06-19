using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket
{
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
