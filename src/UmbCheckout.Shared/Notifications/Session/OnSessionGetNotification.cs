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
        public string? SessionKey { get; }
        public Models.Basket Basket { get; }
        public UmbCheckoutConfiguration? Configuration { get; }
        public OnSessionGetNotification(HttpContext httpContext, string? sessionId, Models.Basket basket, UmbCheckoutConfiguration? configuration)
        {
            HttpContext = httpContext;
            Basket = basket;
            SessionKey = sessionId;
            Configuration = configuration;
        }
    }
}