using System.Text.Json.Serialization;

namespace UmbCheckout.Backoffice.Models
{
    public class UmbCheckoutMetaData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
}
