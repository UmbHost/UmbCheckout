using UmbCheckout.Shared.Models;

namespace UmbCheckout.Backoffice.Models
{
    /// <summary>
    /// The configuration as edited in the backoffice
    /// </summary>
    public class ConfigurationModel
    {
        public IEnumerable<LinkPickerModel> SuccessPageUrl { get; set; } = [];

        public IEnumerable<LinkPickerModel> CancelPageUrl { get; set; } = [];

        public string CurrencyCode { get; set; } = string.Empty;

        public bool EnableShipping { get; set; }

        public bool StoreBasketInCookie { get; set; }

        public int BasketInCookieExpiry { get; set; } = 30;

        public bool StoreBasketInDatabase { get; set; }

        public int BasketInDatabaseExpiry { get; set; } = 30;
    }

    /// <summary>
    /// The configuration returned to the backoffice, including information about the installation
    /// </summary>
    public class ConfigurationResponseModel
    {
        public IEnumerable<MultiUrlPicker> SuccessPageUrl { get; set; } = [];

        public IEnumerable<MultiUrlPicker> CancelPageUrl { get; set; } = [];

        public string CurrencyCode { get; set; } = string.Empty;

        public bool EnableShipping { get; set; }

        public bool StoreBasketInCookie { get; set; }

        public int BasketInCookieExpiry { get; set; } = 30;

        public bool StoreBasketInDatabase { get; set; }

        public int BasketInDatabaseExpiry { get; set; } = 30;

        public bool IsLicensed { get; set; }

        public bool HasUmbracoApplicationUrlSet { get; set; }
    }

    /// <summary>
    /// A link as posted by the Umbraco backoffice link picker, where any value can be null
    /// </summary>
    public class LinkPickerModel
    {
        public string? Name { get; set; }

        public string? Type { get; set; }

        public string? Unique { get; set; }

        public string? Url { get; set; }

        public string? QueryString { get; set; }

        public string? Target { get; set; }

        public string? Icon { get; set; }

        public bool? Published { get; set; }

        public bool? Trashed { get; set; }

        public MultiUrlPicker ToMultiUrlPicker() => new()
        {
            Name = Name ?? string.Empty,
            Type = Type,
            Unique = Unique,
            Url = Url ?? string.Empty,
            QueryString = QueryString,
            Target = Target,
            Icon = Icon ?? string.Empty,
            Published = Published ?? false,
            Trashed = Trashed ?? false
        };
    }
}
