using Umbraco.Cms.Core.Models.PublishedContent;

namespace UmbCheckout.Core.Interfaces
{
    /// <summary>
    /// A service to handle the Get of the Currency
    /// </summary>
    public interface ICurrencyService
    {
        /// <summary>
        /// Gets the cultures currency or global currency
        /// </summary>
        /// <param name="currencyNodeKey">The key of the node which contains the currency code for the current culture</param>
        /// <returns>The currency code for the current culture or global currency</returns>
        /// <exception cref="InvalidOperationException"></exception>
        [Obsolete("Use GetCurrencyAsync(Guid?, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<string> GetCurrencyAsync(Guid? currencyNodeKey = null);

        /// <summary>As <see cref="GetCurrencyAsync(Guid?)"/>, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
        /// <remarks>Implementations written before the token overloads existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<string> GetCurrencyAsync(Guid? currencyNodeKey, CancellationToken cancellationToken) => GetCurrencyAsync(currencyNodeKey);
#pragma warning restore CS0618

        /// <summary>
        /// The currency <paramref name="content"/> is sold in: the umbCheckoutCurrencyCode property on its site root, else the
        /// UmbCheckout Configuration currency, else (as before this member existed, with a warning logged once) the current UI culture's currency.
        /// This is the currency checkout charges; it never comes from the browser.
        /// </summary>
        /// <remarks>
        /// The default implementation (for implementations written before this member existed) returns <see cref="GetCurrencyAsync"/>'s
        /// result when it is shaped like an ISO 4217 code (three ASCII letters), else null. Payment providers still check the code is one they support.
        /// </remarks>
        /// <returns>The upper-case ISO code, or null when no currency is configured (or <paramref name="content"/> is null).</returns>
        [Obsolete("Use GetCurrencyForContentAsync(IPublishedContent, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        async Task<string?> GetCurrencyForContentAsync(IPublishedContent? content)
        {
#pragma warning disable CS0618 // the old default implementation, kept for implementations written before the token overload
            var code = (await GetCurrencyAsync())?.Trim();
#pragma warning restore CS0618
            return code is { Length: 3 } && code.All(c => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z')
                ? code.ToUpperInvariant()
                : null;
        }

        /// <summary>As <see cref="GetCurrencyForContentAsync(IPublishedContent)"/>, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
        /// <remarks>Implementations written before the token overloads existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<string?> GetCurrencyForContentAsync(IPublishedContent? content, CancellationToken cancellationToken) => GetCurrencyForContentAsync(content);
#pragma warning restore CS0618
    }
}
