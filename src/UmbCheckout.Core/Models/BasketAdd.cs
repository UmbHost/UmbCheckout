namespace UmbCheckout.Core.Models
{
    /// <summary>
    /// The base model for the Basket SurfaceController
    /// </summary>
    public class BasketAdd
    {
        public Guid Key { get; set; }

        public int Quantity { get; set; } = 1;
    }
}
