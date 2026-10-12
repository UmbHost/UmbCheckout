namespace UmbCheckout.Core.ManagementApi
{
    /// <summary>
    /// Decides whether a namespace belongs to a package's API document, so each document's operation and schema ID handlers stay scoped to their own package.
    /// </summary>
    internal static class UmbCheckoutNamespaceScope
    {
        public static bool Matches(string? ns, string prefix, string? excludedPrefix)
            => ns is not null
               && (ns == prefix || ns.StartsWith(prefix + ".", StringComparison.Ordinal))
               && (excludedPrefix is null || !(ns == excludedPrefix || ns.StartsWith(excludedPrefix + ".", StringComparison.Ordinal)));
    }
}
