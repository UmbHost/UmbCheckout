using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket
{
    public class OnBasketClearStartedNotification : INotification
    {
        public Models.Basket Basket { get; set; }

        public OnBasketClearStartedNotification(Models.Basket basket)
        {
            Basket = basket;
        }
    }
}
