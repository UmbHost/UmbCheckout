namespace UmbCheckout.Shared.Models
{
    /// <summary>
    /// The Basket model
    /// </summary>
    public class Basket
    {
        private decimal _total;
        public string Id { get; set; } = string.Empty;

        public string SessionId { get; set; } = string.Empty;

        public string? SiteRootCulture { get; set; } = null;

        public string? CustomerReferenceId { get; set; } = null;

        public Customer? Customer { get; set; } = null;

        public IEnumerable<LineItem> LineItems { get; set; } = Enumerable.Empty<LineItem>();

        /// <summary>
        /// The number of lines in the basket, not the number of items: two of one product and one of another is 2.
        /// For the quantity of items (what a basket link shows), use <c>IBasketService.TotalItems</c>, which sums each line's quantity.
        /// </summary>
        public long ItemCount => LineItems.Count();

        public decimal Total
        {
            get
            {
                if (_total == default)
                {
                    _total = LineItems.Sum(lineItem => lineItem.Price * lineItem.Quantity);
                }

                return _total;
            }

            set => _total = value;
        }

        public Dictionary<string, string> MetaData { get; set; } = new();
    }
}
