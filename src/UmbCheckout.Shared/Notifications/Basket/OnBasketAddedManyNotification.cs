using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket
{
    public class OnBasketAddedManyNotification : INotification
    {
        public IEnumerable<LineItem> LineItems { get; }
        public Models.Basket Basket { get; set; }

        public OnBasketAddedManyNotification(IEnumerable<LineItem> lineItems, Models.Basket basket)
        {
            LineItems = lineItems;
            Basket = basket;
        }
    }
}
