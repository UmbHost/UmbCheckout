using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.PaymentProvider
{
    /// <summary>
    /// Notification which is triggered before the payment provider session is created
    /// </summary>
    public class OnProviderCreateSessionStartedNotification : INotification
    {
        public Models.Basket Basket { get; }

        public OnProviderCreateSessionStartedNotification(Models.Basket basket)
        {
            Basket = basket;
        }
    }
}
