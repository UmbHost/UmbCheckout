namespace UmbCheckout.Core.Interfaces
{
    /// <summary>
    /// What identifies the current request's basket: the site root culture and the currency the site sells in.
    /// </summary>
    public interface IBasketContext
    {
        /// <summary>The culture of the current page's site root (from its domains), or null.</summary>
        string? SiteRootCulture { get; }

        /// <summary>The currency the current page sells in, else the Configuration currency, else null.</summary>
        [Obsolete("Use GetCurrencyAsync(CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        Task<string?> GetCurrencyAsync();

        /// <summary>The current page's currency, stopping when <paramref name="cancellationToken"/> is cancelled.</summary>
        /// <remarks>Implementations written before this overload existed ignore the token.</remarks>
#pragma warning disable CS0618 // forwards to the old overload so implementations written before the token overload keep working
        Task<string?> GetCurrencyAsync(CancellationToken cancellationToken) => GetCurrencyAsync();
#pragma warning restore CS0618
    }
}
