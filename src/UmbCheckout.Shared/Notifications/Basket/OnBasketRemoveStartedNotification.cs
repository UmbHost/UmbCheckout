using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket
{
    public class OnBasketRemoveStartedNotification : INotification
    {
        public Models.Basket Basket { get; }

        public Guid Key { get; }

        public OnBasketRemoveStartedNotification(Guid key, Models.Basket basket)
        {
            Key = key;
            Basket = basket;
        }
    }
}
