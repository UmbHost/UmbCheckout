using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket
{
    /// <summary>
    /// Notification which is triggered before a line item is reduced in the Basket
    /// </summary>
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
