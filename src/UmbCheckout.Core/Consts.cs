namespace UmbCheckout.Core
{
    public static class Consts
    {
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
    }
}
