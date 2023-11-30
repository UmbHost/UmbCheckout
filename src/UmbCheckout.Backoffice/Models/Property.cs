using Newtonsoft.Json;

namespace UmbCheckout.Backoffice.Models
{
    /// <summary>
    /// A backoffice configuration property
    /// </summary>
    internal class Property
    {
        [JsonProperty(PropertyName = "alias")]
        public string Alias { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "value")]
        public object Value { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "view")]
        public string View { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "validation")]
        public Validation Validation { get; set; } = new();

        [JsonProperty("config")]
        public Config? Config { get; set; }
    }

    internal class Validation
    {
        [JsonProperty(PropertyName = "mandatory")]
        public bool Mandatory { get; set; }

        [JsonProperty(PropertyName = "mandatoryMessage")]
        public string MandatoryMessage { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "pattern")]
        public string? Pattern { get; set; }

        [JsonProperty(PropertyName = "patternMessage")]
        public string PatternMessage { get; set; } = string.Empty;
    }

    internal class Config
    {
        [JsonProperty("minNumber")]
        public int MinNumber { get; set; }


        [JsonProperty("maxNumber")]
        public int MaxNumber { get; set; }


        [JsonProperty("overlaySize")] 
        public string? OverlaySize { get; set; }


        [JsonProperty("hideAnchor")]
        public bool HideAnchor { get; set; }

        [JsonProperty("ignoreUserStartNodes")]
        public bool IgnoreUserStartNodes { get; set; }

        [JsonProperty("maxChars")]
        public int MaxChars { get; set; }
    }
}