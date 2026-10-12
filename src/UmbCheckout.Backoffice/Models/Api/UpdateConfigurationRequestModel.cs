using System.ComponentModel.DataAnnotations;

namespace UmbCheckout.Backoffice.Models.Api
{
    public class UpdateConfigurationRequestModel
    {
        [Required, StringLength(3, MinimumLength = 0)]
        public string CurrencyCode { get; set; } = string.Empty;
        public Guid? SuccessPageKey { get; set; }
        public Guid? CancelPageKey { get; set; }
        public bool EnableShipping { get; set; }
        public bool StoreBasketInCookie { get; set; }
        public int BasketInCookieExpiry { get; set; } = 30;
        public bool StoreBasketInDatabase { get; set; }
        public int BasketInDatabaseExpiry { get; set; } = 30;
    }
}
