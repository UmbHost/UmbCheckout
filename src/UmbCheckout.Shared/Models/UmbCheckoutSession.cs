namespace UmbCheckout.Shared.Models
{
    /// <summary>
    /// UmbCheckout session model
    /// </summary>
    public class UmbCheckoutSession
    {
        public Basket Basket { get; set; } = new();
    }
}
