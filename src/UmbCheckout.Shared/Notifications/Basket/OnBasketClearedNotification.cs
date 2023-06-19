using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket
{
    public class OnBasketClearedNotification : INotification
    {
        public Models.Basket Basket { get; set; }

        public OnBasketClearedNotification(Models.Basket basket)
        {
            Basket = basket;
        }
    }
}
