using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket
{
    /// <summary>
    /// Notification which is triggered after the Basket is cleared
    /// </summary>
    public class OnBasketClearedNotification : INotification
    {
        public Models.Basket Basket { get; set; }

        public OnBasketClearedNotification(Models.Basket basket)
        {
            Basket = basket;
        }
    }
}
