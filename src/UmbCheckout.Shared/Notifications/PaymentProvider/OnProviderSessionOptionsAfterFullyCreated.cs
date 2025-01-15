using Stripe.Checkout;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Stripe.Services
{
    /// <summary>
    ///     Notification which is triggered just after the session options have been fully created with all options
    /// </summary>
    public class OnProviderSessionOptionsAfterFullyCreated : INotification
    {
        public SessionCreateOptions Options { get; set; }

        public OnProviderSessionOptionsAfterFullyCreated(SessionCreateOptions options)
        {
            Options = options;
        }
    }
}