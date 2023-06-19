
using Microsoft.AspNetCore.Http;

namespace UmbCheckout.Shared.Helpers
{
    /// <summary>
    /// A helper to Get or Set a cookie
    /// </summary>
    public class CookieHelper
    {
        /// <summary>
        /// Gets a cookie
        /// </summary>
        /// <param name="httpContext">The current HttpContext</param>
        /// <param name="key">The cookie key</param>
        /// <returns></returns>
        public static string? Get(HttpContext httpContext, string key)
        {
            return httpContext.Request.Cookies[key];
        }

        /// <summary>
        /// Sets a cookie
        /// </summary>
        /// <param name="httpContext">The current HttpContext</param>
        /// <param name="key">The cookie key</param>
        /// <param name="value">The cookie value</param>
        /// <param name="expireTime">The cookie expiry</param>
        public static void Set(HttpContext httpContext, string key, string value, int? expireTime = null)
        {
            var option = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = expireTime.HasValue 
                    ? DateTime.Now.AddMinutes(expireTime.Value) 
                    : DateTime.Now.AddHours(12)
            };

            httpContext.Response.Cookies.Append(key, value, option);
        }

        /// <summary>
        /// Removes a cookie
        /// </summary>
        /// <param name="httpContext">The current HttpContext</param>
        /// <param name="key">The cookie key</param>
        public static void Remove(HttpContext httpContext, string key)
        {
            httpContext.Response.Cookies.Delete(key);
        }
    }
}
