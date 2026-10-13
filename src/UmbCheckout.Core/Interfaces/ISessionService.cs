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
        [Obsolete("Use Get(CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<UmbCheckoutSession> Get();

        /// <summary>Gets the current request's currency basket session, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
        /// <remarks>Implementations written before this overload existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<UmbCheckoutSession> Get(CancellationToken cancellationToken) => Get();
#pragma warning restore CS0618

        /// <summary>
        /// Gets the UmbCheckout Session holding the basket in <paramref name="currencyCode"/> (a basket belongs to one currency)
        /// </summary>
        /// <param name="currencyCode">The basket's currency</param>
        /// <returns>The UmbCheckout Session for that currency</returns>
        [Obsolete("Use Get(string?, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<UmbCheckoutSession> Get(string? currencyCode);

        /// <summary>Gets the session holding the basket in <paramref name="currencyCode"/>, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<UmbCheckoutSession> Get(string? currencyCode, CancellationToken cancellationToken) => Get(currencyCode);
#pragma warning restore CS0618

        /// <summary>
        /// Gets the UmbCheckout Session holding the basket for <paramref name="siteRootCulture"/> and <paramref name="currencyCode"/>,
        /// for code that runs before the page (and so its culture) is known, such as the success handler while routing.
        /// </summary>
        /// <remarks>Implementations written before this overload existed fall back to the request's culture.</remarks>
        [Obsolete("Use Get(string?, string?, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<UmbCheckoutSession> Get(string? siteRootCulture, string? currencyCode) => Get(currencyCode);

        /// <summary>Gets the session holding the basket for <paramref name="siteRootCulture"/> and <paramref name="currencyCode"/>, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
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

        /// <summary>Stores <paramref name="basket"/>, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<UmbCheckoutSession> Update(Basket basket, CancellationToken cancellationToken) => Update(basket);
#pragma warning restore CS0618

        /// <summary>
        /// Clears the current request's currency basket
        /// </summary>
        /// <returns>True when the basket is still present afterwards</returns>
        [Obsolete("Use Clear(CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<bool> Clear();

        /// <summary>Clears the current request's currency basket, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<bool> Clear(CancellationToken cancellationToken) => Clear();
#pragma warning restore CS0618

        /// <summary>
        /// Clears the basket in <paramref name="currencyCode"/>; other currencies' baskets are kept
        /// </summary>
        /// <param name="currencyCode">The currency of the basket to clear</param>
        /// <returns>True when the basket is still present afterwards</returns>
        [Obsolete("Use Clear(string?, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<bool> Clear(string? currencyCode);

        /// <summary>Clears the basket in <paramref name="currencyCode"/>, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<bool> Clear(string? currencyCode, CancellationToken cancellationToken) => Clear(currencyCode);
#pragma warning restore CS0618

        /// <summary>
        /// Clears the basket of <paramref name="siteRootCulture"/> in <paramref name="currencyCode"/>, for callers that run
        /// before the request's page is known (e.g. while routing); other baskets are kept
        /// </summary>
        /// <param name="siteRootCulture">The basket's site root culture</param>
        /// <param name="currencyCode">The basket's currency</param>
        /// <returns>True when the basket is still present afterwards</returns>
        [Obsolete("Use Clear(string?, string?, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<bool> Clear(string? siteRootCulture, string? currencyCode);

        /// <summary>Clears the basket of <paramref name="siteRootCulture"/> in <paramref name="currencyCode"/>, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<bool> Clear(string? siteRootCulture, string? currencyCode, CancellationToken cancellationToken) => Clear(siteRootCulture, currencyCode);
#pragma warning restore CS0618
    }
}
