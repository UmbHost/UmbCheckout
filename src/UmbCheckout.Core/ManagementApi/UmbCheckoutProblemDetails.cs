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

        /// <summary>The ProblemDetails type of a save or delete a notification handler cancelled; the detail is the handler's message.</summary>
        public const string CancelledType = "urn:umbcheckout:cancelled";

        /// <summary>The title of a save a notification handler cancelled (the handler's message is the detail).</summary>
        public const string SaveCancelledTitle = "Save cancelled";

        /// <summary>The title of a delete a notification handler cancelled (the handler's message is the detail).</summary>
        public const string DeleteCancelledTitle = "Delete cancelled";
    }
}
