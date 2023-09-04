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

        [JsonPropertyName("cancelPageUrl")]
        public IEnumerable<MultiUrlPicker> CancelPageUrl { get; set; } = Enumerable.Empty<MultiUrlPicker>();

        [JsonPropertyName("storeBasketInCookie")]
        public string StoreBasketInCookie { get; set; } = "False";

        [JsonPropertyName("basketInCookieExpiry")]
        public int BasketInCookieExpiry { get; set; } = 30;

        [JsonPropertyName("storeBasketInDatabase")]
        public string StoreBasketInDatabase { get; set; } = "False";

        [JsonPropertyName("basketInDatabaseExpiry")]
        public int BasketInDatabaseExpiry { get; set; } = 30;

        [JsonPropertyName("enableShipping")] 
        public string EnableShipping { get; set; } = "False";
    }
}