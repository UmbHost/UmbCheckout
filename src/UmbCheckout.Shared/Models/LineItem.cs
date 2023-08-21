using UmbCheckout.Shared.Extensions;

namespace UmbCheckout.Shared.Models
{
    /// <summary>
    /// The Line Item model
    /// </summary>
    public class LineItem
    {
        private string? _currencyCode = string.Empty;
        public Guid Key { get; set; } = Guid.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; } = string.Empty;

        public string? CurrencyCode
        {
            get => _currencyCode?.ToUpper();
            set => _currencyCode = value;
        }

        public decimal Price { get; set; }

        public string CurrencyPrice => Price.FormatCurrency(CurrencyCode);

        public long Quantity { get; set; }
    }
}
