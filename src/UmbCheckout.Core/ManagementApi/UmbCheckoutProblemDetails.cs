namespace UmbCheckout.Core.ManagementApi
{
    public static class UmbCheckoutProblemDetails
    {
        public const string UnlicensedTitle = "Unlicensed";
        public const string UnlicensedDetail = "This feature requires an UmbCheckout license.";
        public const string NotFoundTitle = "Not found";
        public const string StripeErrorTitle = "Stripe error";
        public const string StripeNotConfiguredTitle = "Stripe is not configured";
        public const string StripeUnreachableTitle = "Stripe could not be reached";
        public const string InvalidValueTitle = "Invalid value";

        /// <summary>The ProblemDetails type of a save or delete a notification handler cancelled; the title is the handler's message.</summary>
        public const string CancelledType = "urn:umbcheckout:cancelled";
    }
}
