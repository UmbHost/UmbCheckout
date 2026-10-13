using System.Globalization;
using UmbCheckout.Shared.Extensions;
using Xunit;

namespace UmbCheckout.Tests
{
    public class ExtensionTests
    {
        private static readonly CultureInfo EnGb = CultureInfo.GetCultureInfo("en-GB");
        private static readonly CultureInfo DeDe = CultureInfo.GetCultureInfo("de-DE");

        [Fact]
        public void ConvertToBoolean()
        {
            const string stringOne = "1";
            const string stringTrue = "true";
            const string stringZero = "0";
            const string stringFalse = "false";
            const string stringTrueMultiCase = "TrUe";
            const string stringFalseMultiCase = "FaLsE";

            Assert.True(stringOne.ToBoolean());
            Assert.True(stringTrue.ToBoolean());
            Assert.True(stringTrueMultiCase.ToBoolean());
            Assert.False(stringZero.ToBoolean());
            Assert.False(stringFalse.ToBoolean());
            Assert.False(stringFalseMultiCase.ToBoolean());
        }

        [Fact]
        public void FormatCurrency_GbpOnAnEnglishSite()
            => Assert.Equal("£1.99", 1.99m.FormatCurrency("GBP", EnGb));

        [Fact]
        public void FormatCurrency_GbpOnAGermanSite()
        {
            var formatted = 1234.5m.FormatCurrency("GBP", DeDe);
            Assert.Contains("1.234,50", formatted);
            Assert.Contains("£", formatted);
        }

        [Fact]
        public void FormatCurrency_UsesTheSiteCulturesSeparatorsNotTheCurrencysCountry()
        {
            // Used to pick an arbitrary culture per currency (EUR could come out Catalan).
            var formatted = 1234.5m.FormatCurrency("EUR", EnGb);
            Assert.Contains("1,234.50", formatted);
            Assert.Contains("€", formatted);
            Assert.Contains("1.234,50", 1234.5m.FormatCurrency("EUR", DeDe));
        }

        [Theory]
        [InlineData("en-GB")]
        [InlineData("de-DE")]
        public void FormatCurrency_ZeroDecimalCurrencyHasNoDecimals(string culture)
        {
            var formatted = 500m.FormatCurrency("JPY", CultureInfo.GetCultureInfo(culture));
            Assert.Contains("500", formatted);
            Assert.DoesNotContain(".", formatted);
            Assert.DoesNotContain(",", formatted);
        }

        [Fact]
        public void FormatCurrency_ThreeDecimalCurrencyShowsThree()
        {
            Assert.Contains("1.235", 1.235m.FormatCurrency("KWD", EnGb));
            Assert.Contains("1,235", 1.235m.FormatCurrency("KWD", DeDe));
        }

        [Theory]
        [InlineData("2.005", "GBP", "2.01")]
        [InlineData("10.5", "JPY", "11")]
        public void FormatCurrency_RoundsAsTheChargeIsRounded(string amount, string currency, string expected)
        {
            // A price with more decimals than its currency is shown as StripeCurrency.Round charges it (half away from zero).
            var price = decimal.Parse(amount, CultureInfo.InvariantCulture);
            Assert.Contains(expected, price.FormatCurrency(currency, EnGb));
            Assert.Equal(price.FormatCurrency(currency, EnGb), UmbCheckout.Stripe.Money.StripeCurrency.Round(price, currency).FormatCurrency(currency, EnGb));
        }

        [Fact]
        public void FormatCurrency_UnknownCodeIsShownAfterTheAmount()
            => Assert.Equal("10.00 XYZ", 10m.FormatCurrency("XYZ", EnGb));

        [Fact]
        public void FormatCurrency_NoCodeIsAPlainAmount()
            => Assert.Equal("10.00", 10m.FormatCurrency(null, EnGb));

        [Fact]
        public void FormatCurrency_WorksInTheInvariantCulture()
            => Assert.Contains("1.99", 1.99m.FormatCurrency("GBP", CultureInfo.InvariantCulture));

        [Fact]
        public void FormatCurrency_WithoutACultureUsesTheCurrentCulture()
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = DeDe;
                Assert.Contains("1.234,50", 1234.5m.FormatCurrency("GBP"));
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }
    }
}
