using UmbCheckout.Shared.Models;

namespace UmbCheckout.Core.Interfaces
{
    /// <summary>
    /// A service to handle the Get, Update and Clearing of the Session
    /// </summary>
    public interface ISessionService
    {
        /// <summary>
        /// Gets the current UmbCheckout Session for the current request's currency
        /// </summary>
        /// <returns>The current UmbCheckout Session</returns>
        Task<UmbCheckoutSession> Get();

        /// <summary>
        /// Gets the UmbCheckout Session holding the basket in <paramref name="currencyCode"/> (a basket belongs to one currency)
        /// </summary>
        /// <param name="currencyCode">The basket's currency</param>
        /// <returns>The UmbCheckout Session for that currency</returns>
        Task<UmbCheckoutSession> Get(string? currencyCode);

        /// <summary>
        /// Stores <paramref name="basket"/> under its site root culture and currency
        /// </summary>
        /// <param name="basket">The Basket</param>
        /// <returns>The current UmbCheckout Session</returns>
        Task<UmbCheckoutSession> Update(Basket basket);

        /// <summary>
        /// Clears the current request's currency basket
        /// </summary>
        /// <returns>True when the basket is still present afterwards</returns>
        Task<bool> Clear();

        /// <summary>
        /// Clears the basket in <paramref name="currencyCode"/>; other currencies' baskets are kept
        /// </summary>
        /// <param name="currencyCode">The currency of the basket to clear</param>
        /// <returns>True when the basket is still present afterwards</returns>
        Task<bool> Clear(string? currencyCode);
    }
}
