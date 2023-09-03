using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.PaymentProvider
{
    /// <summary>
    /// Notification which is triggered after the payment provider session is cleared
    /// </summary>
    public class OnProviderSessionClearedNotification : INotification
    {
        public string SessionId { get; }

        public OnProviderSessionClearedNotification(string id)
        {
            SessionId = id;
        }
    }
}
