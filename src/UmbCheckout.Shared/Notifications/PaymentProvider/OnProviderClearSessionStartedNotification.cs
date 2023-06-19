using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.PaymentProvider
{
    public class OnProviderClearSessionStartedNotification : INotification
    {
        public string SessionId { get; }

        public OnProviderClearSessionStartedNotification(string id)
        {
            SessionId = id;
        }
    }
}
