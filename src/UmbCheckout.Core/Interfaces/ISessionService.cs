using UmbCheckout.Shared.Models;

namespace UmbCheckout.Core.Interfaces
{
    /// <summary>
    /// A service to handle the Get, Update and Clearing of the Session
    /// </summary>
    public interface ISessionService
    {
        /// <summary>
        /// Gets the current UmbCheckout Session: the basket of the current request's site root culture and currency
        /// </summary>
        /// <returns>The current UmbCheckout Session</returns>
        Task<UmbCheckoutSession> Get();

        /// <summary>
        /// Gets the UmbCheckout Session holding the basket in <paramref name="currencyCode"/> (a basket belongs to one currency)
        /// </summary>
        /// <remarks>The default implementation (for implementations written before baskets were per currency) returns <see cref="Get()"/>.</remarks>
        /// <param name="currencyCode">The basket's currency</param>
        /// <returns>The UmbCheckout Session for that currency</returns>
        Task<UmbCheckoutSession> Get(string? currencyCode) => Get();

        /// <summary>
        /// Stores <paramref name="basket"/> under its site root culture and currency
        /// </summary>
        /// <param name="basket">The Basket</param>
        /// <returns>The current UmbCheckout Session</returns>
        Task<UmbCheckoutSession> Update(Basket basket);

        /// <summary>
        /// Clears the current request's basket (its site root culture and currency); other baskets are kept
        /// </summary>
        /// <returns>True when the basket is still present afterwards</returns>
        Task<bool> Clear();

        /// <summary>
        /// Clears the basket in <paramref name="currencyCode"/> for the current request's site root culture; other baskets are kept
        /// </summary>
        /// <remarks>The default implementation (for implementations written before baskets were per currency) calls <see cref="Clear()"/>.</remarks>
        /// <param name="currencyCode">The currency of the basket to clear</param>
        /// <returns>True when the basket is still present afterwards</returns>
        Task<bool> Clear(string? currencyCode) => Clear();

        /// <summary>
        /// Clears the basket of <paramref name="siteRootCulture"/> in <paramref name="currencyCode"/>, for callers that run
        /// before the request's page is known (e.g. while routing); other baskets are kept
        /// </summary>
        /// <remarks>The default implementation (for implementations written before baskets were per currency) calls <see cref="Clear(string?)"/>.</remarks>
        /// <param name="siteRootCulture">The basket's site root culture</param>
        /// <param name="currencyCode">The basket's currency</param>
        /// <returns>True when the basket is still present afterwards</returns>
        Task<bool> Clear(string? siteRootCulture, string? currencyCode) => Clear(currencyCode);
    }
}
