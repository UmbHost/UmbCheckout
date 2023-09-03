using System.Text.Json.Serialization;

namespace UmbCheckout.Shared.Models
{
    /// <summary>
    /// Model for the Success / Cancel page picker in the backoffice
    /// </summary>
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
