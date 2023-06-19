using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket
{
    /// <summary>
    /// Notification which is triggered after a line items is reduced in the Basket
    /// </summary>
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
