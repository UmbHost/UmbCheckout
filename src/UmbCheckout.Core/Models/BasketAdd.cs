namespace UmbCheckout.Core.Models
{
    /// <summary>
    /// The base model for the Basket SurfaceController
    /// </summary>
    public class BasketAdd
    {
        public Guid Key { get; set; }

        /// <summary>Ignored: the currency is set by the server (the product's site root, then Configuration), never by the browser.</summary>
        [Obsolete("Ignored. The currency is set by the server (ICurrencyService.GetCurrencyForContentAsync); a posted currency is never charged.")]
        public string? CurrencyCode { get; set; } = string.Empty;

        public int Quantity { get; set; } = 1;
    }
}
