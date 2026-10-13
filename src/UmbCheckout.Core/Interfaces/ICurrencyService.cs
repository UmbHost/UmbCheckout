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

        /// <summary>Gets the cultures currency or global currency, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
        /// <remarks>Implementations written before the token overloads existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<string> GetCurrencyAsync(Guid? currencyNodeKey, CancellationToken cancellationToken) => GetCurrencyAsync(currencyNodeKey);
#pragma warning restore CS0618

        /// <summary>
        /// The currency <paramref name="content"/> is sold in: the umbCheckoutCurrencyCode property on its site root, else the
        /// UmbCheckout Configuration currency. This is the currency checkout charges; it never comes from the browser.
        /// </summary>
        /// <returns>The upper-case ISO code, or null (logged) when no currency is configured.</returns>
        [Obsolete("Use GetCurrencyForContentAsync(IPublishedContent, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<string?> GetCurrencyForContentAsync(IPublishedContent content);

        /// <summary>The currency <paramref name="content"/> is sold in, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<string?> GetCurrencyForContentAsync(IPublishedContent content, CancellationToken cancellationToken) => GetCurrencyForContentAsync(content);
#pragma warning restore CS0618
    }
}
