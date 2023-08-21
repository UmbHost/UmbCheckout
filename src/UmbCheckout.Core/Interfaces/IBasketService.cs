using UmbCheckout.Shared.Models;

namespace UmbCheckout.Core.Interfaces
{
    /// <summary>
    /// A service which handles all things around the basket
    /// </summary>
    public interface IBasketService
    {
        /// <summary>
        /// Gets the Basket
        /// </summary>
        /// <returns>The Basket</returns>
        Task<Basket> Get();

        /// <summary>
        /// Adds an item to the Basket
        /// </summary>
        /// <param name="item">Item to be added to the Basket</param>
        /// <returns>The updated Basket</returns>
        Task<Basket> Add(LineItem item);

        /// <summary>
        /// Adds multiple items to the Basket
        /// </summary>
        /// <param name="items">Items to be added to the Basket</param>
        /// <returns>The updated Basket</returns>
        Task<Basket> Add(IEnumerable<LineItem> items);

        /// <summary>
        /// Reduces the item quantity by 1 or removes it if the last item
        /// </summary>
        /// <param name="key">Item to be reduced in the Basket</param>
        /// <returns>The updated Basket</returns>
        Task<Basket> Reduce(Guid key);

        /// <summary>
        /// Removes an item from the Basket
        /// </summary>
        /// <param name="key">Item to be removed from the Basket</param>
        /// <returns>The updated Basket</returns>
        Task<Basket> Remove(Guid key);

        /// <summary>
        /// Removes multiple items from the Basket
        /// </summary>
        /// <param name="keys">Items to be removed from the Basket</param>
        /// <returns>The updated Basket</returns>
        Task<Basket> Remove(IEnumerable<Guid> keys);

        /// <summary>
        /// Removes items from the Basket and clears any Basket cookies
        /// </summary>
        /// <returns>The updated Basket</returns>
        Task<Basket> Clear();

        /// <summary>
        /// Gets the total items held within the Basket
        /// </summary>
        /// <returns>Count of items in the Basket</returns>
        Task<long> TotalItems();

        /// <summary>
        /// Gets the sub total of the Basket
        /// </summary>
        /// <returns>The sub total of the Basket</returns>
        Task<decimal> SubTotal();
    }
}
