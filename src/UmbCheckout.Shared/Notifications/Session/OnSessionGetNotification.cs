using Microsoft.AspNetCore.Http;
using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Session
{
    /// <summary>
    /// Notification which is triggered after the UmbCheckout session is collected
    /// </summary>
    public class OnSessionGetNotification : INotification
    {
        public HttpContext HttpContext { get; }
        public UmbCheckoutSession? Session { get; }
        public UmbCheckoutConfiguration? Configuration { get; }
        public OnSessionGetNotification(HttpContext httpContext, UmbCheckoutSession? session, UmbCheckoutConfiguration? configuration)
        {
            HttpContext = httpContext;
            Session = session;
            Configuration = configuration;
        }
    }
}