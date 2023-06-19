using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.PaymentProvider
{
    public class OnProviderSessionCreatedNotification : INotification
    {
        public Models.Basket Basket { get; }

        public OnProviderSessionCreatedNotification(Models.Basket basket)
        {
            Basket = basket;
        }
    }
}
