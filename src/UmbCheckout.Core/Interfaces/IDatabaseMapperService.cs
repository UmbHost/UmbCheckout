using UmbCheckout.Core.Pocos;
using UmbCheckout.Shared.Models;

namespace UmbCheckout.Core.Interfaces
{
    /// <summary>
    /// A service which handles the mapping the basket to and from the database
    /// </summary>
    public interface IDatabaseMapperService
    {
        /// <summary>
        /// Converts a UmbCheckoutBasket to a Basket
        /// </summary>
        /// <param name="umbCheckoutBasket">The UmbCheckoutBasket to be converted</param>
        /// <returns>Basket conversion</returns>
        Basket? ToBasket(UmbCheckoutBasket umbCheckoutBasket);
    }
}
