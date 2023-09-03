using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.PaymentProvider
{
    /// <summary>
    /// Notification which is triggered after the payment provider session is created
    /// </summary>
    public class OnProviderSessionCreatedNotification : INotification
    {
        public Models.Basket Basket { get; }

        public OnProviderSessionCreatedNotification(Models.Basket basket)
        {
            Basket = basket;
        }
    }
}
