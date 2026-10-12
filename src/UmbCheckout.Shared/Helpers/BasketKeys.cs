namespace UmbCheckout.Shared.Helpers
{
    /// <summary>
    /// Where a basket is stored: one per site root culture and currency (a basket belongs to exactly one currency).
    /// </summary>
    public static class BasketKeys
    {
        /// <summary>The session key for the basket of <paramref name="siteRootCulture"/> in <paramref name="currencyCode"/>.</summary>
        public static string Session(string? siteRootCulture, string? currencyCode) => Consts.SessionKey + Suffix(siteRootCulture, currencyCode);

        /// <summary>The cookie name for the basket of <paramref name="siteRootCulture"/> in <paramref name="currencyCode"/>.</summary>
        public static string Cookie(string? siteRootCulture, string? currencyCode) => Consts.SessionBasketKey + Suffix(siteRootCulture, currencyCode);

        /// <summary>The session key used before baskets were per currency (culture only).</summary>
        public static string LegacySession(string? siteRootCulture) => Consts.SessionKey + siteRootCulture;

        /// <summary>The cookie name used before baskets were per currency (culture only).</summary>
        public static string LegacyCookie(string? siteRootCulture) => Consts.SessionBasketKey + siteRootCulture;

        private static string Suffix(string? siteRootCulture, string? currencyCode)
            => string.IsNullOrWhiteSpace(currencyCode)
                ? siteRootCulture ?? string.Empty
                : $"{siteRootCulture}_{currencyCode.Trim().ToUpperInvariant()}";
    }
}
