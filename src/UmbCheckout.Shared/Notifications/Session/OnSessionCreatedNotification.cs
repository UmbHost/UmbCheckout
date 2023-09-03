using Microsoft.AspNetCore.Http;
using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Session
{
    /// <summary>
    /// Notification which is triggered after the UmbCheckout session is created
    /// </summary>
    public class OnSessionCreatedNotification : INotification
    {
        public HttpContext HttpContext { get; }
        public string EncryptedBasket { get; set; }
        public string SessionKey { get; set; }
        public UmbCheckoutConfiguration? Configuration { get; }
        public OnSessionCreatedNotification(HttpContext httpContext, string sessionKey, string encryptedBasket, UmbCheckoutConfiguration? configuration)
        {
            HttpContext = httpContext;
            EncryptedBasket = encryptedBasket;
            SessionKey = sessionKey;
            Configuration = configuration;
        }
    }
}
