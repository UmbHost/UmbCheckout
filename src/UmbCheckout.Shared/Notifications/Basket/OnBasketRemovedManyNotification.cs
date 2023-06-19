using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket
{
    public class OnBasketRemovedManyNotification : INotification
    {
        public IEnumerable<Guid> Keys { get; }
        public Models.Basket Basket { get; set; }

        public OnBasketRemovedManyNotification(IEnumerable<Guid> keys, Models.Basket basket)
        {
            Keys = keys;
            Basket = basket;
        }
    }
}
