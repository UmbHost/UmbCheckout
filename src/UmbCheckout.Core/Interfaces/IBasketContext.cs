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
        Task<string?> GetCurrencyAsync();
    }
}
