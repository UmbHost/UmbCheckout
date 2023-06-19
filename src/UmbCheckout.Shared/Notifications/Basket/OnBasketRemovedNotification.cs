using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket
{
    /// <summary>
    /// Notification which is triggered after a line items is removed from the Basket
    /// </summary>
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
