using UmbCheckout.Shared.Models;

namespace UmbCheckout.Core.Interfaces
{
    /// <summary>
    /// A service to handle the Get, Update and Clearing of the Session
    /// </summary>
    public interface ISessionService
    {
        /// <summary>
        /// Gets the current UmbCheckout Session
        /// </summary>
        /// <returns>The current UmbCheckout Session</returns>
        Task<UmbCheckoutSession> Get();

        /// <summary>
        /// Updates the current UmbCheckout Session
        /// </summary>
        /// <param name="basket">The Basket</param>
        /// <returns>The current UmbCheckout Session</returns>
        Task<UmbCheckoutSession> Update(Basket basket);

        /// <summary>
        /// Clears the current UmbCheckout Session
        /// </summary>
        /// <returns>True if session not found, false if session is found</returns>
        Task<bool> Clear();
    }
}
