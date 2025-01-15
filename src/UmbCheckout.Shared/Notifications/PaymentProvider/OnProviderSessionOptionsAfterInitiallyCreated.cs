using Stripe.Checkout;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.PaymentProvider
{
    /// <summary>
    ///     Notification which is triggered just after the initial session options are created
    /// </summary>
    public class OnProviderSessionOptionsAfterInitiallyCreated : INotification
    {
        public SessionCreateOptions Options { get; set; }

        public OnProviderSessionOptionsAfterInitiallyCreated(SessionCreateOptions options)
        {
            Options = options;
        }
    }
}
