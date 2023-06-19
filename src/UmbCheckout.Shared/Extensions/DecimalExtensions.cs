using System.Globalization;

namespace UmbCheckout.Shared.Extensions
{
    public static class DecimalExtension
    {
        private static readonly Dictionary<string, CultureInfo> ISOCurrenciesToACultureMap =
            CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(c => new { c, new RegionInfo(c.Name).ISOCurrencySymbol })
                .GroupBy(x => x.ISOCurrencySymbol)
                .ToDictionary(g => g.Key, g => g.First().c, StringComparer.OrdinalIgnoreCase);

        public static string FormatCurrency(this decimal amount, string currencyCode)
        {
            if (ISOCurrenciesToACultureMap.TryGetValue(currencyCode, out var culture))
                return amount.ToString("C", culture);
            return amount.ToString("F");
        }
    }
}
