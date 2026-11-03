using System.Globalization;
using UmbCheckout.Shared.Money;

namespace UmbCheckout.Shared.Extensions
{
    /// <summary>
    /// Extension which formats a decimal as currency with currency symbol
    /// </summary>
    public static class DecimalExtensions
    {
        private static readonly Dictionary<string, CultureInfo> ISOCurrenciesToACultureMap =
            CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(c => new { c, new RegionInfo(c.Name).ISOCurrencySymbol })
                .GroupBy(x => x.ISOCurrencySymbol)
                .ToDictionary(g => g.Key, g => g.First().c, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Formats <paramref name="amount"/> in the site culture (separators, symbol position) with the currency's own symbol and
        /// decimal places (JPY 0, KWD 3). An unknown code is shown after the amount; no code gives a plain amount.
        /// </summary>
        /// <param name="culture">The culture to format in; defaults to the current culture, which Umbraco sets per request.</param>
        public static string FormatCurrency(this decimal amount, string? currencyCode, CultureInfo? culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;

            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                return amount.ToString("N2", culture);
            }

            currencyCode = currencyCode.Trim();

            // The decimal places come from the same table as the charge (CurrencyDecimals), never from .NET's culture for the currency:
            // the first culture for AFN, ALL, LAK, LBP, MMK, RSD, SOS or YER shows 0 decimals, while they are charged with 2.
            var digits = CurrencyDecimals.ChargeableDigits(currencyCode);
            if (!ISOCurrenciesToACultureMap.TryGetValue(currencyCode, out var currencyCulture))
            {
                return $"{amount.ToString("N" + digits, culture)} {currencyCode.ToUpperInvariant()}";
            }

            // The currency's culture gives only its symbol; the site culture gives the pattern and separators.
            var format = (NumberFormatInfo)culture.NumberFormat.Clone();
            format.CurrencySymbol = currencyCulture.NumberFormat.CurrencySymbol;
            format.CurrencyDecimalDigits = digits;
            return amount.ToString("C", format);
        }
    }
}
