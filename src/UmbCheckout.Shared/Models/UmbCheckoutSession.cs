namespace UmbCheckout.Shared.Models
{
    /// <summary>
    /// UmbCheckout session model
    /// </summary>
    public class UmbCheckoutSession
    {
        public string? SiteRootCulture { get; set; }
        public Basket Basket { get; set; } = new();
    }
}
