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
        /// it returns from that session (not a basket started later, or a shared link).
        /// </summary>
        public string? CheckoutSessionId { get; set; } = null;

        public Customer? Customer { get; set; } = null;

        public IEnumerable<LineItem> LineItems { get; set; } = Enumerable.Empty<LineItem>();

        public long ItemCount => LineItems.Count();

        /// <summary>Always computed from the lines; never stored, so it cannot go stale.</summary>
        [JsonIgnore]
        public decimal Total => LineItems.Sum(lineItem => lineItem.Price * lineItem.Quantity);

        public Dictionary<string, string> MetaData { get; set; } = new();
    }
}
