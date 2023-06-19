using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket
{
    public class OnBasketReducedNotification : INotification
    {
        public Guid Key { get; }
        public Models.Basket Basket { get; set; }

        public OnBasketReducedNotification(Guid key, Models.Basket basket)
        {
            Key = key;
            Basket = basket;
        }
    }
}
