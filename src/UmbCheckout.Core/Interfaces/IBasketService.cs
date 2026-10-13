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
        [Obsolete("Use Get(CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<Basket> Get();

        /// <summary>As <see cref="Get()"/>, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
        /// <remarks>Implementations written before the token overloads existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<Basket> Get(CancellationToken cancellationToken) => Get();
#pragma warning restore CS0618

        /// <summary>
        /// Adds an item to the basket in its own currency (a basket belongs to one currency); a line already there for the same product gets the added quantity and the item's price
        /// </summary>
        /// <param name="item">Item to be added to the Basket</param>
        /// <exception cref="ArgumentException">A line has no currency or a quantity outside 1 to <see cref="Shared.Consts.MaxLineQuantity"/> (also once added to the line already in the basket), it would be a product beyond <see cref="Shared.Consts.MaxLineItems"/>, or (for several items) there are none or they are in more than one currency. The basket is left unchanged.</exception>
        /// <returns>The updated Basket</returns>
        [Obsolete("Use Add(LineItem, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<Basket> Add(LineItem item);

        /// <summary>As <see cref="Add(LineItem)"/>, stopping when <paramref name="cancellationToken"/> is cancelled. The token stops the add before the basket is stored, never part-way through.</summary>
        /// <remarks>Implementations written before the token overloads existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<Basket> Add(LineItem item, CancellationToken cancellationToken) => Add(item);
#pragma warning restore CS0618

        /// <summary>
        /// Adds multiple items, all in one currency, to that currency's basket; lines already there for the same products get the added quantities and the items' prices
        /// </summary>
        /// <param name="items">Items to be added to the Basket</param>
        /// <exception cref="ArgumentException">A line has no currency or a quantity outside 1 to <see cref="Shared.Consts.MaxLineQuantity"/> (also once added to the line already in the basket), it would be a product beyond <see cref="Shared.Consts.MaxLineItems"/>, or (for several items) there are none or they are in more than one currency. The basket is left unchanged.</exception>
        /// <returns>The updated Basket</returns>
        [Obsolete("Use Add(IEnumerable<LineItem>, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<Basket> Add(IEnumerable<LineItem> items);

        /// <summary>As <see cref="Add(IEnumerable{LineItem})"/>, stopping when <paramref name="cancellationToken"/> is cancelled. The token stops the add before the basket is stored, never part-way through.</summary>
        /// <remarks>Implementations written before the token overloads existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<Basket> Add(IEnumerable<LineItem> items, CancellationToken cancellationToken) => Add(items);
#pragma warning restore CS0618

        /// <summary>
        /// Reduces the item quantity by 1 or removes it if the last item
        /// </summary>
        /// <param name="key">Item to be reduced in the Basket</param>
        /// <returns>The updated Basket</returns>
        [Obsolete("Use Reduce(Guid, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<Basket> Reduce(Guid key);

        /// <summary>As <see cref="Reduce(Guid)"/>, stopping when <paramref name="cancellationToken"/> is cancelled. The token stops the change before the basket is stored, never part-way through.</summary>
        /// <remarks>Implementations written before the token overloads existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<Basket> Reduce(Guid key, CancellationToken cancellationToken) => Reduce(key);
#pragma warning restore CS0618

        /// <summary>
        /// Removes an item from the Basket
        /// </summary>
        /// <param name="key">Item to be removed from the Basket</param>
        /// <returns>The updated Basket</returns>
        [Obsolete("Use Remove(Guid, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<Basket> Remove(Guid key);

        /// <summary>As <see cref="Remove(Guid)"/>, stopping when <paramref name="cancellationToken"/> is cancelled. The token stops the change before the basket is stored, never part-way through.</summary>
        /// <remarks>Implementations written before the token overloads existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<Basket> Remove(Guid key, CancellationToken cancellationToken) => Remove(key);
#pragma warning restore CS0618

        /// <summary>
        /// Removes multiple items from the Basket
        /// </summary>
        /// <param name="keys">Items to be removed from the Basket</param>
        /// <returns>The updated Basket</returns>
        [Obsolete("Use Remove(IEnumerable<Guid>, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<Basket> Remove(IEnumerable<Guid> keys);

        /// <summary>As <see cref="Remove(IEnumerable{Guid})"/>, stopping when <paramref name="cancellationToken"/> is cancelled. The token stops the change before the basket is stored, never part-way through.</summary>
        /// <remarks>Implementations written before the token overloads existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<Basket> Remove(IEnumerable<Guid> keys, CancellationToken cancellationToken) => Remove(keys);
#pragma warning restore CS0618

        /// <summary>
        /// Removes items from the Basket and clears any Basket cookies
        /// </summary>
        /// <returns>The updated Basket</returns>
        [Obsolete("Use Clear(CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<Basket> Clear();

        /// <summary>As <see cref="Clear()"/>, stopping when <paramref name="cancellationToken"/> is cancelled. The token stops the clear before the basket is stored, never part-way through.</summary>
        /// <remarks>Implementations written before the token overloads existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<Basket> Clear(CancellationToken cancellationToken) => Clear();
#pragma warning restore CS0618

        /// <summary>
        /// Gets the total items held within the Basket
        /// </summary>
        /// <returns>Count of items in the Basket</returns>
        [Obsolete("Use TotalItems(CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<long> TotalItems();

        /// <summary>As <see cref="TotalItems()"/>, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
        /// <remarks>Implementations written before the token overloads existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<long> TotalItems(CancellationToken cancellationToken) => TotalItems();
#pragma warning restore CS0618

        /// <summary>
        /// Gets the sub total of the Basket
        /// </summary>
        /// <returns>The sub total of the Basket</returns>
        [Obsolete("Use SubTotal(CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<decimal> SubTotal();

        /// <summary>As <see cref="SubTotal()"/>, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
        /// <remarks>Implementations written before the token overloads existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<decimal> SubTotal(CancellationToken cancellationToken) => SubTotal();
#pragma warning restore CS0618
    }
}
