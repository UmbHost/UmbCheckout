using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.PaymentProvider
{
    /// <summary>
    /// Notification which is triggered before the payment provider session is collected
    /// </summary>
    public class OnProviderGetSessionStartedNotification : INotification
    {
        public string SessionId { get; }

        public OnProviderGetSessionStartedNotification(string id)
        {
            SessionId = id;
        }
    }
}
