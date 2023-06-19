using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.PaymentProvider
{
    /// <summary>
    /// Notification which is triggered after the payment provider session is collected
    /// </summary>
    public class OnProviderGetSessionNotification : INotification
    {
        public string SessionId { get; }

        public OnProviderGetSessionNotification(string id)
        {
            SessionId = id;
        }
    }
}
