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

        /// <summary>The site root culture of the basket that was cleared.</summary>
        public string? SiteRootCulture { get; init; }

        /// <summary>The currency of the basket that was cleared (a basket belongs to one currency).</summary>
        public string? CurrencyCode { get; init; }
        public OnSessionClearedNotification(HttpContext httpContext, string? sessionId, UmbCheckoutConfiguration? configuration)
        {
            SessionKey = sessionId;
            HttpContext = httpContext;
            Configuration = configuration;
        }
    }
}
