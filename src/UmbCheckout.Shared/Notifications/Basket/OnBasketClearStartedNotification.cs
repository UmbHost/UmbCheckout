using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket
{
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
