using UmbCheckout.Shared.Models;

namespace UmbCheckout.Shared.Helpers
{
    /// <summary>The baskets a legacy basket splits into, and the lines that had no currency.</summary>
    public sealed record BasketSplit(IReadOnlyList<Basket> Baskets, IReadOnlyList<LineItem> Dropped);

    /// <summary>
    /// Splits a basket saved before baskets were per currency into one basket per line currency.
    /// </summary>
    public static class BasketSplitter
    {
        public static BasketSplit ByCurrency(Basket legacy)
        {
            var dropped = legacy.LineItems.Where(lineItem => string.IsNullOrWhiteSpace(lineItem.CurrencyCode)).ToList();
            var baskets = legacy.LineItems
                .Where(lineItem => !string.IsNullOrWhiteSpace(lineItem.CurrencyCode))
                .GroupBy(lineItem => lineItem.CurrencyCode!.Trim().ToUpperInvariant())
                .Select(lines => new Basket
                {
                    // A database id cannot belong to several baskets: each is stored afresh on its next update.
                    Id = string.Empty,
                    SessionId = legacy.SessionId,
                    SiteRootCulture = legacy.SiteRootCulture,
                    CurrencyCode = lines.Key,
                    CustomerReferenceId = legacy.CustomerReferenceId,
                    Customer = legacy.Customer,
                    MetaData = new Dictionary<string, string>(legacy.MetaData),
                    LineItems = lines.ToList()
                })
                .ToList();

            return new BasketSplit(baskets, dropped);
        }
    }
}
