using Microsoft.AspNetCore.Http;
using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Session
{
    public class OnSessionUpdatedNotification : INotification
    {
        public HttpContext HttpContext { get; }
        public Models.Basket Basket { get; }
        public string EncryptedBasket { get; set; }
        public string SessionKey { get; set; }
        public UmbCheckoutConfiguration? Configuration { get; }

        public OnSessionUpdatedNotification(HttpContext httpContext, string sessionKey, Models.Basket basket, string encryptedBasket, UmbCheckoutConfiguration? configuration)
        {
            HttpContext = httpContext;
            Basket = basket;
            EncryptedBasket = encryptedBasket;
            SessionKey = sessionKey;
            Configuration = configuration;
        }
    }
}
