namespace UmbCheckout.Shared.Money
{
    /// <summary>
    /// The number of decimal places each currency is charged and displayed in: one table for the charge (Stripe's minor units) and the display
    /// (<see cref="Extensions.DecimalExtensions.FormatCurrency(decimal, string?, System.Globalization.CultureInfo?)"/>), so they can't disagree.
    /// Source: https://docs.stripe.com/currencies (currencies are two-decimal unless listed otherwise).
    /// </summary>
    public static class CurrencyDecimals
    {
        /// <summary>
        /// Stripe's zero-decimal currencies. ISK and UGX are deliberately absent: although zero-decimal, Stripe's API
        /// still represents them as two-decimal values (5 ISK is sent as 500), so they use the two-decimal default.
        /// </summary>
        private static readonly HashSet<string> ZeroDecimal = new(StringComparer.OrdinalIgnoreCase)
        {
            "bif", "clp", "djf", "gnf", "jpy", "kmf", "krw", "mga", "pyg", "rwf", "vnd", "vuv", "xaf", "xof", "xpf"
        };

        /// <summary>Currencies whose minor unit is a thousandth (ISO 4217 exponent 3).</summary>
        private static readonly HashSet<string> ThreeDecimal = new(StringComparer.OrdinalIgnoreCase)
        {
            "bhd", "jod", "kwd", "omr", "tnd"
        };

        /// <summary>
        /// Zero-decimal currencies Stripe still represents as two-decimal values (5 ISK is sent as 500).
        /// Docs: "You can't charge fractions of ISK" / UGX likewise.
        /// </summary>
        private static readonly HashSet<string> WholeUnitsOnly = new(StringComparer.OrdinalIgnoreCase)
        {
            "isk", "ugx"
        };

        /// <summary>The number of decimal places in the currency's minor unit as Stripe's API represents it (JPY 0, GBP 2, KWD 3, ISK 2).</summary>
        /// <exception cref="ArgumentException">The code is null, empty or white space.</exception>
        public static int MinorUnitDigits(string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
            {
                throw new ArgumentException("A currency code is required.", nameof(currency));
            }

            currency = currency.Trim();
            if (ZeroDecimal.Contains(currency)) return 0;
            if (ThreeDecimal.Contains(currency)) return 3;
            return 2;
        }

        /// <summary>The number of decimal places an amount in <paramref name="currency"/> is charged and displayed with (JPY 0, GBP 2, KWD 3, ISK 0).</summary>
        /// <exception cref="ArgumentException">The code is null, empty or white space.</exception>
        public static int ChargeableDigits(string currency)
        {
            var digits = MinorUnitDigits(currency);
            return WholeUnitsOnly.Contains(currency.Trim()) ? 0 : digits;
        }
    }
}
