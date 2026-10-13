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

        /// <summary>Gets the Basket, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
        /// <remarks>Implementations written before the token overloads existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<Basket> Get(CancellationToken cancellationToken) => Get();
#pragma warning restore CS0618

        /// <summary>
        /// Adds an item to the Basket
        /// </summary>
        /// <param name="item">Item to be added to the Basket</param>
        /// <returns>The updated Basket</returns>
        /// <exception cref="ArgumentException">A line has no currency, a quantity outside 1–<see cref="Shared.Consts.MaxLineQuantity"/>, a product beyond <see cref="Shared.Consts.MaxLineItems"/>, or (for several items) more than one currency.</exception>
        [Obsolete("Use Add(LineItem, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<Basket> Add(LineItem item);

        /// <summary>Adds an item to the Basket, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<Basket> Add(LineItem item, CancellationToken cancellationToken) => Add(item);
#pragma warning restore CS0618

        /// <summary>
        /// Adds multiple items to the Basket
        /// </summary>
        /// <param name="items">Items to be added to the Basket</param>
        /// <returns>The updated Basket</returns>
        /// <exception cref="ArgumentException">A line has no currency, a quantity outside 1–<see cref="Shared.Consts.MaxLineQuantity"/>, a product beyond <see cref="Shared.Consts.MaxLineItems"/>, or (for several items) more than one currency.</exception>
        [Obsolete("Use Add(IEnumerable<LineItem>, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<Basket> Add(IEnumerable<LineItem> items);

        /// <summary>Adds multiple items to the Basket, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
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

        /// <summary>Reduces the item quantity by 1 or removes it if the last item, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
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

        /// <summary>Removes an item from the Basket, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
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

        /// <summary>Removes multiple items from the Basket, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<Basket> Remove(IEnumerable<Guid> keys, CancellationToken cancellationToken) => Remove(keys);
#pragma warning restore CS0618

        /// <summary>
        /// Removes items from the Basket and clears any Basket cookies
        /// </summary>
        /// <returns>The updated Basket</returns>
        [Obsolete("Use Clear(CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<Basket> Clear();

        /// <summary>Removes items from the Basket and clears any Basket cookies, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<Basket> Clear(CancellationToken cancellationToken) => Clear();
#pragma warning restore CS0618

        /// <summary>
        /// Gets the total items held within the Basket: the sum of every line's quantity (unlike <see cref="Basket.ItemCount"/>, which counts lines)
        /// </summary>
        /// <returns>Count of items in the Basket</returns>
        [Obsolete("Use TotalItems(CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<long> TotalItems();

        /// <summary>Gets the total items held within the Basket, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<long> TotalItems(CancellationToken cancellationToken) => TotalItems();
#pragma warning restore CS0618

        /// <summary>
        /// Gets the sub total of the Basket
        /// </summary>
        /// <returns>The sub total of the Basket</returns>
        [Obsolete("Use SubTotal(CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<decimal> SubTotal();

        /// <summary>Gets the sub total of the Basket, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<decimal> SubTotal(CancellationToken cancellationToken) => SubTotal();
#pragma warning restore CS0618
    }
}
