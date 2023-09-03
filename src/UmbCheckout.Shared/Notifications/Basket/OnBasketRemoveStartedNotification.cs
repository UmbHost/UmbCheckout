using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket
{
    /// <summary>
    /// Notification which is triggered before a line item is removed from the Basket
    /// </summary>
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
