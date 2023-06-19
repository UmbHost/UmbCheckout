using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket
{
    public class OnBasketRemovedNotification : INotification
    {
        public Guid Key { get; }
        public Models.Basket Basket { get; set; }

        public OnBasketRemovedNotification(Guid key, Models.Basket basket)
        {
            Key = key;
            Basket = basket;
        }
    }
}
