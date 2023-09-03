using Microsoft.AspNetCore.Http;
using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Session
{
    /// <summary>
    /// Notification which is triggered after the UmbCheckout session is cleared
    /// </summary>
    public class OnSessionClearedNotification : INotification
    {
        public HttpContext HttpContext { get; }
        public string? SessionKey { get; }
        public UmbCheckoutConfiguration? Configuration { get; }
        public OnSessionClearedNotification(HttpContext httpContext, string? sessionId, UmbCheckoutConfiguration? configuration)
        {
            SessionKey = sessionId;
            HttpContext = httpContext;
            Configuration = configuration;
        }
    }
}
