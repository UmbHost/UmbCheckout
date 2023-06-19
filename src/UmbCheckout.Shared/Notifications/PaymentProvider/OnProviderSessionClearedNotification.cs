using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.PaymentProvider
{
    public class OnProviderSessionClearedNotification : INotification
    {
        public string SessionId { get; }

        public OnProviderSessionClearedNotification(string id)
        {
            SessionId = id;
        }
    }
}
