namespace UmbCheckout.Shared
{
    /// <summary>
    /// UmbCheckout Constants
    /// </summary>
    public static class Consts
    {
        public const string PackageName = "UmbCheckout";
        
        public const string SessionKey = "umbCheckoutBasketSessionId";

        public const string SessionMode = "payment";

        public static class PropertyAlias
        {
            public const string PriceAlias = "umbCheckoutPrice";

            public const string DescriptionAlias = "umbCheckoutDescription";

            public const string FallbackDescriptionAlias = "description";

            public const string MetaDataAlias = "umbCheckoutMetaData";

            public const string TaxRatesAlias = "umbCheckoutTaxRates";
        }

        public static class LocalizationKeys
        {
            public const string Area = "umbcheckout";
            public const string DisabledUnlicensed = "disabled_unlicensed";
            public const string UnlicensedWarning = "unlicensed_warning";
            public const string UnlicensedNotificationWarningTitle = "unlicensed_notification_warning_title";
            public const string UnlicensedNotificationWarningMessage = "unlicensed_notification_warning_message";
            public const string HelpSupport = "help_support";
            public const string MadeBy = "made_by";
            public const string Configuration = "configuration";
            public const string LicenseStatus = "license_status";
            public const string LicenseAddon = "license_addon";
            public const string LicenseAddonName = "license_addon_name";
            public const string LicenseDetails = "license_details";
            public const string RegistrationDate = "registration_date";
            public const string ValidDomains = "valid_domains";
            public const string ValidPath = "valid_path";
            public const string RecheckLicense = "recheck_license";
            public const string ExpiryDate = "expiry_date";
            public const string SuccessPageUrlLabel = "success_page_url_label";
            public const string SuccessPageUrlDescription = "success_page_url_description";
            public const string CancelPageUrlLabel = "cancel_page_url_label";
            public const string CancelPageUrlDescription = "cancel_page_url_description";
            public const string EnableShippingLabel = "enable_shipping_label";
            public const string EnableShippingDescription = "enable_shipping_description";
            public const string StoreBasketCookieLabel = "store_basket_cookie_label";
            public const string StoreBasketCookieDescription = "store_basket_cookie_description";
            public const string StoreBasketCookieExpiryLabel = "store_basket_cookie_expiry_label";
            public const string StoreBasketCookieExpiryDescription = "store_basket_cookie_expiry_description";
            public const string StoreBasketDatabaseLabel = "store_basket_database_label";
            public const string StoreBasketDatabaseDescription = "store_basket_database_description";
            public const string StoreBasketDatabaseExpiryLabel = "store_basket_database_expiry_label";
            public const string StoreBasketDatabaseExpiryDescription = "store_basket_database_expiry_description";
            public const string DocumentationDescription = "documentation_description";
            public const string GitHubReportIssues = "github_report_issues";
            public const string SupportTicket = "support_ticket";
            public const string ShippingRates = "shipping_rates";
            public const string ShippingRate = "shipping_rate";
            public const string TaxRate = "tax_rate";
            public const string TaxRates = "tax_rates";
        }
    }
}
