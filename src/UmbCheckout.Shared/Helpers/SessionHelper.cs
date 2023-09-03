using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace UmbCheckout.Shared.Helpers
{
    /// <summary>
    /// A helper to Set or Get a session item
    /// </summary>
    public static class SessionHelper
    {
        /// <summary>
        /// Sets a session item
        /// </summary>
        /// <param name="session">The current Session</param>
        /// <param name="key">The session key</param>
        /// <param name="value">The session value</param>
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        /// <summary>
        /// Gets a session item
        /// </summary>
        /// <typeparam name="T">Type to be returned</typeparam>
        /// <param name="session">The current Session</param>
        /// <param name="key">The session key</param>
        /// <returns></returns>
        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
