namespace UmbCheckout.Shared.Models
{
    /// <summary>
    /// Configuration model
    /// </summary>
    public class UmbCheckoutConfiguration
    {
        public int Id { get; set; }

        public Guid Key { get; set; } = Guid.NewGuid();

        public string CurrencyCode { get; set; } = string.Empty;

        public IEnumerable<MultiUrlPicker> SuccessPageUrl { get; set; } = Enumerable.Empty<MultiUrlPicker>();

        public IEnumerable<MultiUrlPicker> CancelPageUrl { get; set; } = Enumerable.Empty<MultiUrlPicker>();

        public bool StoreBasketInCookie { get; set; }

        public bool StoreBasketInDatabase { get; set; }

        public int BasketInDatabaseExpiry { get; set; } = 30;

        public int BasketInCookieExpiry { get; set; } = 30;

        public bool EnableShipping { get; set; }
    }
}
