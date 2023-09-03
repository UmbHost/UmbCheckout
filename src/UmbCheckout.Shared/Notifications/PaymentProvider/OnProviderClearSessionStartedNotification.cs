using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.PaymentProvider
{
    /// <summary>
    /// Notification which is triggered before the payment provider session is cleared
    /// </summary>
    public class OnProviderClearSessionStartedNotification : INotification
    {
        public string SessionId { get; }

        public OnProviderClearSessionStartedNotification(string id)
        {
            SessionId = id;
        }
    }
}
