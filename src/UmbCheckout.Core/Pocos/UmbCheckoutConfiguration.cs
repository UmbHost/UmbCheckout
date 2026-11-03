namespace UmbCheckout.Core.Pocos
{
    /// <summary>
    /// The configuration row (table UmbCheckoutConfiguration, mapped in UmbCheckoutConfigurationConfiguration)
    /// </summary>
    internal class UmbCheckoutConfiguration
    {
        public int Id { get; set; }

        /// <summary>Null on rows created before the Key column existed; assigned on the next save.</summary>
        public Guid? Key { get; set; } = Guid.NewGuid();

        public string? CurrencyCode { get; set; }

        public string SuccessPageUrl { get; set; } = string.Empty;

        public string CancelPageUrl { get; set; } = string.Empty;

        public bool StoreBasketInCookie { get; set; }

        public bool StoreBasketInDatabase { get; set; }

        public int BasketInDatabaseExpiry { get; set; } = 30;

        public int BasketInCookieExpiry { get; set; } = 30;

        public bool EnableShipping { get; set; }
    }
}
