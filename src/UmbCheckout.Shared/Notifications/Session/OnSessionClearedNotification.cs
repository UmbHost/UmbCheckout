using Microsoft.AspNetCore.Http;
using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Session
{
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
