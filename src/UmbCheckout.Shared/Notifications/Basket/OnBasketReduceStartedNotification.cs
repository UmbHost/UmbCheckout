using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket
{
    public class OnBasketReduceStartedNotification : INotification
    {
        public Models.Basket Basket { get; }
        public Guid Key { get; }

        public OnBasketReduceStartedNotification(Guid key, Models.Basket basket)
        {
            Basket = basket;
            Key = key;
        }
    }
}
