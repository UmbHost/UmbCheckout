using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket
{
    public class OnBasketRemoveManyStartedNotification : INotification
    {
        public Models.Basket Basket { get; }
        public IEnumerable<Guid> Keys { get; }

        public OnBasketRemoveManyStartedNotification(IEnumerable<Guid> keys, Models.Basket basket)
        {
            Keys = keys;
            Basket = basket;
        }
    }
}
