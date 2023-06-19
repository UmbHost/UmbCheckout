using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.PaymentProvider
{
    public class OnProviderGetSessionStartedNotification : INotification
    {
        public string SessionId { get; }

        public OnProviderGetSessionStartedNotification(string id)
        {
            SessionId = id;
        }
    }
}
