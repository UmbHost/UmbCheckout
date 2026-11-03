using UmbCheckout.Backoffice.Models.Api;
using UmbCheckout.Shared.Models;

namespace UmbCheckout.Backoffice.Mapping
{
    /// <summary>
    /// Maps between the stored configuration (MultiUrlPicker lists, kept for storage and uSync compatibility) and the API models (document keys).
    /// </summary>
    public static class ConfigurationModelMapper
    {
        private const string DocumentUdiPrefix = "umb://document/";

        public static ConfigurationResponseModel ToResponse(UmbCheckoutConfiguration configuration, bool isLicensed) => new()
        {
            CurrencyCode = configuration.CurrencyCode,
            SuccessPageKey = ToKey(configuration.SuccessPageUrl),
            CancelPageKey = ToKey(configuration.CancelPageUrl),
            EnableShipping = configuration.EnableShipping,
            StoreBasketInCookie = configuration.StoreBasketInCookie,
            BasketInCookieExpiry = configuration.BasketInCookieExpiry,
            StoreBasketInDatabase = configuration.StoreBasketInDatabase,
            BasketInDatabaseExpiry = configuration.BasketInDatabaseExpiry,
            IsLicensed = isLicensed
        };

        // Basket storage is a licensed add-on feature; the add-on applies its licence rules when this is saved
        public static UmbCheckoutConfiguration ToConfiguration(UpdateConfigurationRequestModel model, IEnumerable<MultiUrlPicker> successPage, IEnumerable<MultiUrlPicker> cancelPage) => new()
        {
            CurrencyCode = model.CurrencyCode.ToUpperInvariant(),
            SuccessPageUrl = successPage,
            CancelPageUrl = cancelPage,
            EnableShipping = model.EnableShipping,
            StoreBasketInCookie = model.StoreBasketInCookie,
            BasketInCookieExpiry = model.BasketInCookieExpiry,
            StoreBasketInDatabase = model.StoreBasketInDatabase,
            BasketInDatabaseExpiry = model.BasketInDatabaseExpiry
        };

        public static MultiUrlPicker ToPicker(Guid key, string name, string url, bool published) => new()
        {
            Udi = $"{DocumentUdiPrefix}{key:N}",
            Name = name,
            Url = url,
            Icon = "icon-document",
            Published = published,
            Trashed = false
        };

        private static Guid? ToKey(IEnumerable<MultiUrlPicker> pickers)
        {
            var udi = pickers.FirstOrDefault()?.Udi;
            if (string.IsNullOrEmpty(udi) || !udi.StartsWith(DocumentUdiPrefix, StringComparison.Ordinal))
            {
                return null;
            }

            return Guid.TryParse(udi[DocumentUdiPrefix.Length..], out var key) ? key : null;
        }
    }
}
