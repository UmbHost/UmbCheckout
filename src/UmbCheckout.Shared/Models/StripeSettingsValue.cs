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
    }
}
