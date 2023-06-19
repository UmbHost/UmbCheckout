using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.PaymentProvider
{
    public class OnProviderCreateSessionStartedNotification : INotification
    {
        public Models.Basket Basket { get; }

        public OnProviderCreateSessionStartedNotification(Models.Basket basket)
        {
            Basket = basket;
        }
    }
}
