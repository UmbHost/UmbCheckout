namespace UmbCheckout.Shared.Models
{
    public class Basket
    {
        public string Id { get; set; } = string.Empty;

        public string SessionId { get; set; } = string.Empty;

        public IEnumerable<LineItem> LineItems { get; set; } = Enumerable.Empty<LineItem>();

        public long ItemCount => LineItems.Count();

        public decimal Total { get; set; }
    }
}
