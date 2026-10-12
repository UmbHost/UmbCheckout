namespace UmbCheckout.Backoffice.Models.Api
{
    public class ConfigurationResponseModel
    {
        public string CurrencyCode { get; set; } = string.Empty;
        public Guid? SuccessPageKey { get; set; }
        public Guid? CancelPageKey { get; set; }
        public bool EnableShipping { get; set; }
        public bool StoreBasketInCookie { get; set; }
        public int BasketInCookieExpiry { get; set; }
        public bool StoreBasketInDatabase { get; set; }
        public int BasketInDatabaseExpiry { get; set; }
        public bool IsLicensed { get; set; }
    }
}
