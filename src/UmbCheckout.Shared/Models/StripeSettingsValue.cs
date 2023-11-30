using System.Text.Json.Serialization;

namespace UmbCheckout.Shared.Models
{
    /// <summary>
    /// The backoffice Stripe settings properties
    /// </summary>
    public class StripeSettingsValue
    {

        [JsonPropertyName("useLiveApiDetails")]
        public string UseLiveApiDetails { get; set; } = "False";

        [JsonPropertyName("collectPhoneNumber")]
        public string CollectPhoneNumber { get; set; } = "False";

        [JsonPropertyName("shippingAllowedCountries")]
        public string? ShippingAllowedCountries { get; set; } = "";
    }
}
