using System.Text.Json.Serialization;

namespace UmbCheckout.Shared.Models
{
    public class MultiUrlPicker
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("udi")]
        public string Udi { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("icon")]
        public string Icon { get; set; } = string.Empty;

        [JsonPropertyName("published")]
        public bool Published { get; set; }

        [JsonPropertyName("trashed")]
        public bool Trashed { get; set; }
    }
}
