namespace UmbCheckout.Core.Models
{
    /// <summary>
    /// The base model for the Basket SurfaceController
    /// </summary>
    public class BasketAdd
    {
        public Guid Id { get; set; }

        public string CurrencyCode { get; set; } = string.Empty;

        public int Quantity { get; set; } = 1;
    }
}
