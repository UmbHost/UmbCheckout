using UmbCheckout.Shared.Models;

namespace UmbCheckout.Shared.Extensions
{
    /// <summary>
    /// The basket's own currency: what its lines are priced (and charged) in.
    /// </summary>
    public static class LineItemExtensions
    {
        /// <summary>The one currency the lines are priced in (upper case), or null when there are none, they disagree, or a line has none.</summary>
        public static string? SingleCurrency(this IEnumerable<LineItem> lineItems)
        {
            var codes = lineItems.Select(lineItem => lineItem.CurrencyCode?.Trim() ?? string.Empty).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            return codes.Count == 1 && codes[0].Length > 0 ? codes[0].ToUpperInvariant() : null;
        }

        /// <summary>True when a line priced in <paramref name="currency"/> can join these lines: every existing line shares it.</summary>
        public static bool AcceptsCurrency(this IEnumerable<LineItem> lineItems, string currency)
            => lineItems.All(lineItem => string.Equals(lineItem.CurrencyCode?.Trim(), currency, StringComparison.OrdinalIgnoreCase));
    }
}
