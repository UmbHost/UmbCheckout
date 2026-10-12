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

        public static UmbCheckoutConfiguration ToConfiguration(UpdateConfigurationRequestModel model, bool isLicensed, IEnumerable<MultiUrlPicker> successPage, IEnumerable<MultiUrlPicker> cancelPage)
        {
            var storeBasketInCookie = model.StoreBasketInCookie;
            var storeBasketInDatabase = model.StoreBasketInDatabase;

            if (!isLicensed)
            {
                storeBasketInCookie = false;
                storeBasketInDatabase = false;
            }
            else if (storeBasketInDatabase)
            {
                storeBasketInCookie = true;
            }

            return new UmbCheckoutConfiguration
            {
                CurrencyCode = model.CurrencyCode.ToUpperInvariant(),
                SuccessPageUrl = successPage,
                CancelPageUrl = cancelPage,
                EnableShipping = model.EnableShipping,
                StoreBasketInCookie = storeBasketInCookie,
                BasketInCookieExpiry = model.BasketInCookieExpiry,
                StoreBasketInDatabase = storeBasketInDatabase,
                BasketInDatabaseExpiry = model.BasketInDatabaseExpiry
            };
        }

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
