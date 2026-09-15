using System.Text.Json.Serialization;
using Umbraco.Cms.Core;

namespace UmbCheckout.Shared.Models
{
    /// <summary>
    /// Model for the Success / Cancel page picker in the backoffice
    /// </summary>
    /// <remarks>
    /// Supports both the Umbraco 14+ link picker format (type / unique) and the legacy format (udi) stored by earlier versions
    /// </remarks>
    public class MultiUrlPicker
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("unique")]
        public string? Unique { get; set; }

        [JsonPropertyName("udi")]
        public string Udi { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("queryString")]
        public string? QueryString { get; set; }

        [JsonPropertyName("target")]
        public string? Target { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; } = string.Empty;

        [JsonPropertyName("published")]
        public bool Published { get; set; }

        [JsonPropertyName("trashed")]
        public bool Trashed { get; set; }

        /// <summary>
        /// Gets the key of the picked document, from either the new or legacy format
        /// </summary>
        /// <returns>The document key, or null if an external link was picked</returns>
        public Guid? GetDocumentKey()
        {
            if (Guid.TryParse(Unique, out var key) && (string.IsNullOrEmpty(Type) || Type == "document"))
            {
                return key;
            }

            if (!string.IsNullOrEmpty(Udi) && UdiParser.TryParse(Udi, out GuidUdi? udi) && udi.EntityType == Constants.UdiEntityType.Document)
            {
                return udi.Guid;
            }

            return null;
        }
    }
}
