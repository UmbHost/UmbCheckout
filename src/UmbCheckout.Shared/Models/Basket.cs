using System.Text.Json.Serialization;

namespace UmbCheckout.Shared.Models
{
    /// <summary>
    /// The Basket model
    /// </summary>
    public class Basket
    {
        public string Id { get; set; } = string.Empty;

        public string SessionId { get; set; } = string.Empty;

        public string? SiteRootCulture { get; set; } = null;

        /// <summary>The one currency every line is priced in (upper-case ISO code); null for a basket created before baskets were per currency.</summary>
        public string? CurrencyCode { get; set; } = null;

        public string? CustomerReferenceId { get; set; } = null;

        /// <summary>
        /// The payment provider's checkout session this basket was last sent to; the success page clears the basket only when
        /// it returns from that session (not a basket started later, or a stale or shared link).
        /// </summary>
        public string? CheckoutSessionId { get; set; } = null;

        public Customer? Customer { get; set; } = null;

        public IEnumerable<LineItem> LineItems { get; set; } = Enumerable.Empty<LineItem>();

        /// <summary>
        /// The number of lines in the basket, not the number of items: two of one product and one of another is 2.
        /// For the quantity of items (what a basket link shows), use <c>IBasketService.TotalItems</c>, which sums each line's quantity.
        /// </summary>
        public long ItemCount => LineItems.Count();

        /// <summary>Always computed from the lines; never stored, so it cannot go stale.</summary>
        /// <remarks>The setter is kept for compatibility and ignores its value: the total is always the lines' sum.</remarks>
        [JsonIgnore]
        public decimal Total
        {
            get => LineItems.Sum(lineItem => lineItem.Price * lineItem.Quantity);
            // ReSharper disable once ValueParameterNotUsed
            set { }
        }

        public Dictionary<string, string> MetaData { get; set; } = new();
    }
}
