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

        /// <summary>Gets the Basket, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
        /// <remarks>Implementations written before the token overloads existed ignore the token.</remarks>
        Task<Basket> Get(CancellationToken cancellationToken) => Get();

        /// <summary>
        /// Adds an item to the Basket
        /// </summary>
        /// <param name="item">Item to be added to the Basket</param>
        /// <returns>The updated Basket</returns>
        /// <exception cref="ArgumentException">A line has no currency, a quantity outside 1–<see cref="Shared.Consts.MaxLineQuantity"/>, a product beyond <see cref="Shared.Consts.MaxLineItems"/>, or (for several items) more than one currency.</exception>
        Task<Basket> Add(LineItem item);

        /// <summary>Adds an item to the Basket, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
        Task<Basket> Add(LineItem item, CancellationToken cancellationToken) => Add(item);

        /// <summary>
        /// Adds multiple items to the Basket
        /// </summary>
        /// <param name="items">Items to be added to the Basket</param>
        /// <returns>The updated Basket</returns>
        /// <exception cref="ArgumentException">A line has no currency, a quantity outside 1–<see cref="Shared.Consts.MaxLineQuantity"/>, a product beyond <see cref="Shared.Consts.MaxLineItems"/>, or (for several items) more than one currency.</exception>
        Task<Basket> Add(IEnumerable<LineItem> items);

        /// <summary>Adds multiple items to the Basket, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
        Task<Basket> Add(IEnumerable<LineItem> items, CancellationToken cancellationToken) => Add(items);

        /// <summary>
        /// Reduces the item quantity by 1 or removes it if the last item
        /// </summary>
        /// <param name="key">Item to be reduced in the Basket</param>
        /// <returns>The updated Basket</returns>
        Task<Basket> Reduce(Guid key);

        /// <summary>Reduces the item quantity by 1 or removes it if the last item, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
        Task<Basket> Reduce(Guid key, CancellationToken cancellationToken) => Reduce(key);

        /// <summary>
        /// Removes an item from the Basket
        /// </summary>
        /// <param name="key">Item to be removed from the Basket</param>
        /// <returns>The updated Basket</returns>
        Task<Basket> Remove(Guid key);

        /// <summary>Removes an item from the Basket, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
        Task<Basket> Remove(Guid key, CancellationToken cancellationToken) => Remove(key);

        /// <summary>
        /// Removes multiple items from the Basket
        /// </summary>
        /// <param name="keys">Items to be removed from the Basket</param>
        /// <returns>The updated Basket</returns>
        Task<Basket> Remove(IEnumerable<Guid> keys);

        /// <summary>Removes multiple items from the Basket, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
        Task<Basket> Remove(IEnumerable<Guid> keys, CancellationToken cancellationToken) => Remove(keys);

        /// <summary>
        /// Removes items from the Basket and clears any Basket cookies
        /// </summary>
        /// <returns>The updated Basket</returns>
        Task<Basket> Clear();

        /// <summary>Removes items from the Basket and clears any Basket cookies, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
        Task<Basket> Clear(CancellationToken cancellationToken) => Clear();

        /// <summary>
        /// Gets the total items held within the Basket
        /// </summary>
        /// <returns>Count of items in the Basket</returns>
        Task<long> TotalItems();

        /// <summary>Gets the total items held within the Basket, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
        Task<long> TotalItems(CancellationToken cancellationToken) => TotalItems();

        /// <summary>
        /// Gets the sub total of the Basket
        /// </summary>
        /// <returns>The sub total of the Basket</returns>
        Task<decimal> SubTotal();

        /// <summary>Gets the sub total of the Basket, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
        Task<decimal> SubTotal(CancellationToken cancellationToken) => SubTotal();
    }
}
