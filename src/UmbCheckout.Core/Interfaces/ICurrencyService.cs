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
        Task<string> GetCurrencyAsync(Guid? currencyNodeKey = null);

        /// <summary>
        /// The currency <paramref name="content"/> is sold in: the umbCheckoutCurrencyCode property on its site root, else the
        /// UmbCheckout Configuration currency. This is the currency checkout charges; it never comes from the browser.
        /// </summary>
        /// <returns>The upper-case ISO code, or null (logged) when no currency is configured.</returns>
        Task<string?> GetCurrencyForContentAsync(IPublishedContent content);
    }
}
