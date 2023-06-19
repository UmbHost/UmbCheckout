namespace UmbCheckout.Stripe.Models
{
    public class StripeSettings
    {
        public string WebHookSecret { get; init; } = string.Empty;

        public string ApiKey { get; init; } = string.Empty;
    }
}
