using System.Text.Json.Serialization;

namespace UmbCheckout.Shared.Models
{
    /// <summary>
    /// The backoffice configuration properties
    /// </summary>
    public class ConfigurationValue
    {
        [JsonPropertyName("successPageUrl")]
        public IEnumerable<MultiUrlPicker> SuccessPageUrl { get; set; } = Enumerable.Empty<MultiUrlPicker>();

        [JsonPropertyName("currencyCode")]
        public string CurrencyCode { get; set; } = string.Empty;

        [JsonPropertyName("cancelPageUrl")]
        public IEnumerable<MultiUrlPicker> CancelPageUrl { get; set; } = Enumerable.Empty<MultiUrlPicker>();

        [JsonPropertyName("storeBasketInCookie")]
        public bool StoreBasketInCookie { get; set; } = false;

        [JsonPropertyName("basketInCookieExpiry")]
        public int BasketInCookieExpiry { get; set; } = 30;

        [JsonPropertyName("storeBasketInDatabase")]
        public bool StoreBasketInDatabase { get; set; } = false;

        [JsonPropertyName("basketInDatabaseExpiry")]
        public int BasketInDatabaseExpiry { get; set; } = 30;

        [JsonPropertyName("enableShipping")] 
        public bool EnableShipping { get; set; } = false;
    }
}