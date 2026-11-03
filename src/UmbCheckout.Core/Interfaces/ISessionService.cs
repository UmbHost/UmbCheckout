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
        [Obsolete("Use Get(CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<UmbCheckoutSession> Get();

        /// <summary>As <see cref="Get()"/>, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
        /// <remarks>Implementations written before the token overloads existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<UmbCheckoutSession> Get(CancellationToken cancellationToken) => Get();
#pragma warning restore CS0618

        /// <summary>
        /// Gets the UmbCheckout Session holding the basket in <paramref name="currencyCode"/> (a basket belongs to one currency)
        /// </summary>
        /// <remarks>The default implementation (for implementations written before baskets were per currency) returns <see cref="Get()"/>.</remarks>
        /// <param name="currencyCode">The basket's currency</param>
        /// <returns>The UmbCheckout Session for that currency</returns>
        [Obsolete("Use Get(string?, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<UmbCheckoutSession> Get(string? currencyCode) => Get();

        /// <summary>As <see cref="Get(string?)"/>, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
        /// <remarks>Implementations written before the token overloads existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<UmbCheckoutSession> Get(string? currencyCode, CancellationToken cancellationToken) => Get(currencyCode);
#pragma warning restore CS0618

        /// <summary>
        /// Gets the UmbCheckout Session holding the basket of <paramref name="siteRootCulture"/> in <paramref name="currencyCode"/>,
        /// for callers that run before the request's page is known (e.g. while routing)
        /// </summary>
        /// <remarks>The default implementation (for implementations written before baskets were per culture and currency) returns <see cref="Get(string?)"/>.</remarks>
        /// <param name="siteRootCulture">The basket's site root culture</param>
        /// <param name="currencyCode">The basket's currency</param>
        /// <returns>The UmbCheckout Session for that culture and currency</returns>
        [Obsolete("Use Get(string?, string?, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<UmbCheckoutSession> Get(string? siteRootCulture, string? currencyCode) => Get(currencyCode);

        /// <summary>As <see cref="Get(string?, string?)"/>, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
        /// <remarks>Implementations written before the token overloads existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<UmbCheckoutSession> Get(string? siteRootCulture, string? currencyCode, CancellationToken cancellationToken) => Get(siteRootCulture, currencyCode);
#pragma warning restore CS0618

        /// <summary>
        /// Stores <paramref name="basket"/> under its site root culture and currency
        /// </summary>
        /// <param name="basket">The Basket</param>
        /// <returns>The current UmbCheckout Session</returns>
        [Obsolete("Use Update(Basket, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<UmbCheckoutSession> Update(Basket basket);

        /// <summary>As <see cref="Update(Basket)"/>, stopping when <paramref name="cancellationToken"/> is cancelled. The token stops the update before the basket is stored, never part-way through.</summary>
        /// <remarks>Implementations written before the token overloads existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<UmbCheckoutSession> Update(Basket basket, CancellationToken cancellationToken) => Update(basket);
#pragma warning restore CS0618

        /// <summary>
        /// Clears the current request's basket (its site root culture and currency); other baskets are kept
        /// </summary>
        /// <returns>True when the basket is still present afterwards</returns>
        [Obsolete("Use Clear(CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<bool> Clear();

        /// <summary>As <see cref="Clear()"/>, stopping when <paramref name="cancellationToken"/> is cancelled. The token stops the clear before it starts, never part-way through.</summary>
        /// <remarks>Implementations written before the token overloads existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<bool> Clear(CancellationToken cancellationToken) => Clear();
#pragma warning restore CS0618

        /// <summary>
        /// Clears the basket in <paramref name="currencyCode"/> for the current request's site root culture; other baskets are kept
        /// </summary>
        /// <remarks>The default implementation (for implementations written before baskets were per currency) calls <see cref="Clear()"/>.</remarks>
        /// <param name="currencyCode">The currency of the basket to clear</param>
        /// <returns>True when the basket is still present afterwards</returns>
        [Obsolete("Use Clear(string?, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<bool> Clear(string? currencyCode) => Clear();

        /// <summary>As <see cref="Clear(string?)"/>, stopping when <paramref name="cancellationToken"/> is cancelled. The token stops the clear before it starts, never part-way through.</summary>
        /// <remarks>Implementations written before the token overloads existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<bool> Clear(string? currencyCode, CancellationToken cancellationToken) => Clear(currencyCode);
#pragma warning restore CS0618

        /// <summary>
        /// Clears the basket of <paramref name="siteRootCulture"/> in <paramref name="currencyCode"/>, for callers that run
        /// before the request's page is known (e.g. while routing); other baskets are kept
        /// </summary>
        /// <remarks>The default implementation (for implementations written before baskets were per currency) calls <see cref="Clear(string?)"/>.</remarks>
        /// <param name="siteRootCulture">The basket's site root culture</param>
        /// <param name="currencyCode">The basket's currency</param>
        /// <returns>True when the basket is still present afterwards</returns>
        [Obsolete("Use Clear(string?, string?, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<bool> Clear(string? siteRootCulture, string? currencyCode) => Clear(currencyCode);

        /// <summary>As <see cref="Clear(string?, string?)"/>, stopping when <paramref name="cancellationToken"/> is cancelled. The token stops the clear before it starts, never part-way through.</summary>
        /// <remarks>Implementations written before the token overloads existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<bool> Clear(string? siteRootCulture, string? currencyCode, CancellationToken cancellationToken) => Clear(siteRootCulture, currencyCode);
#pragma warning restore CS0618
    }
}
