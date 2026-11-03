using System.Globalization;
using UmbCheckout.Shared.Extensions;
using UmbCheckout.Shared.Money;
using UmbCheckout.Stripe.Money;
using Xunit;

namespace UmbCheckout.Tests.Helpers
{
    /// <summary>One decimals table for the charge and the display, so they can never disagree.</summary>
    public class CurrencyDecimalsTests
    {
        private static readonly CultureInfo EnGb = CultureInfo.GetCultureInfo("en-GB");

        [Theory]
        [InlineData("GBP", 2)]
        [InlineData("jpy", 0)]
        [InlineData("KWD", 3)]
        [InlineData("isk", 0)]  // charged in whole units
        [InlineData("ugx", 0)]
        [InlineData(" jpy ", 0)]
        [InlineData(" kwd", 3)]
        [InlineData("Usd ", 2)]
        public void ChargeableDigits_TrimsAndIgnoresCase(string currency, int digits)
            => Assert.Equal(digits, CurrencyDecimals.ChargeableDigits(currency));

        [Theory]
        [InlineData(" isk ", 2)] // Stripe's API represents ISK as two-decimal
        [InlineData("KWD ", 3)]
        [InlineData(" JPY", 0)]
        public void MinorUnitDigits_TrimsAndIgnoresCase(string currency, int digits)
            => Assert.Equal(digits, CurrencyDecimals.MinorUnitDigits(currency));

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void ADecimalsLookupNeedsACode(string? currency)
        {
            Assert.ThrowsAny<ArgumentException>(() => CurrencyDecimals.ChargeableDigits(currency));
            Assert.ThrowsAny<ArgumentException>(() => CurrencyDecimals.MinorUnitDigits(currency));
        }

        [Fact]
        public void TheChargeAndTheDisplayUseTheSameTable()
        {
            // The first .NET culture for AFN, ALL, LAK, LBP, MMK, RSD, SOS or YER shows 0 decimals, while Stripe charges them with 2.
            var codes = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(c => new RegionInfo(c.Name).ISOCurrencySymbol).Distinct()
                .Where(StripeCurrency.IsSupported).ToList();
            Assert.NotEmpty(codes);

            foreach (var code in codes)
            {
                var digits = StripeCurrency.ChargeableDigits(code);
                var formatted = 1m.FormatCurrency(code, CultureInfo.InvariantCulture);
                var fraction = System.Text.RegularExpressions.Regex.Match(formatted, @"\d+(?:\.(\d+))?\s*$").Groups[1].Length;
                Assert.True(digits == fraction, $"{code}: charged with {digits} decimals, displayed with {fraction} ({formatted})");
            }
        }

        [Theory]
        [InlineData("ALL")]
        [InlineData("YER")]
        [InlineData("LBP")]
        public void ACurrencyChargedInTwoDecimalsIsDisplayedInTwo(string code)
            => Assert.EndsWith("1.00", 1m.FormatCurrency(code, EnGb).Trim(), StringComparison.Ordinal);

        [Fact]
        public void FormatCurrency_TrimsTheCode()
        {
            Assert.Equal(1.99m.FormatCurrency("GBP", EnGb), 1.99m.FormatCurrency(" gbp ", EnGb));
            Assert.Equal("500 XYZ", 500m.FormatCurrency(" xyz ", EnGb).Replace(".00", string.Empty));
            Assert.DoesNotContain(".", 500m.FormatCurrency(" jpy ", EnGb));
        }
    }
}
