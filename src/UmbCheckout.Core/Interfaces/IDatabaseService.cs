using UmbCheckout.Shared.Models;

namespace UmbCheckout.Core.Interfaces
{
    /// <summary>
    /// A service which handles storing of the Basket in the database
    /// </summary>
    public interface IDatabaseService
    {
        /// <summary>
        /// Gets the stored Basket
        /// </summary>
        /// <param name="sessionId">The session to retrieve the Basket for</param>
        /// <returns>The stored Basket</returns>
        Task<Basket?> GetBasket(string sessionId);
    }
}
