using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.PaymentProvider
{
    public class OnProviderGetSessionNotification : INotification
    {
        public string SessionId { get; }

        public OnProviderGetSessionNotification(string id)
        {
            SessionId = id;
        }
    }
}
