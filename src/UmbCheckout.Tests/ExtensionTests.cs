using System.Globalization;
using UmbCheckout.Shared.Extensions;
using Xunit;

namespace UmbCheckout.Tests
{
    public class ExtensionTests
    {
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

        private static readonly CultureInfo EnGb = CultureInfo.GetCultureInfo("en-GB");

        [Fact]
        public void FormatCurrency_GbpOnAnEnglishSite()
            => Assert.Equal("£1.99", 1.99m.FormatCurrency("GBP", EnGb));

        [Fact]
        public void FormatCurrency_UsesTheSiteCulturesSeparatorsNotTheCurrencysCountry()
        {
            // Used to pick an arbitrary culture per currency (EUR could come out Catalan).
            var formatted = 1234.5m.FormatCurrency("EUR", EnGb);
            Assert.Contains("1,234.50", formatted);
            Assert.Contains("€", formatted);
            Assert.Contains("1.234,50", 1234.5m.FormatCurrency("EUR", CultureInfo.GetCultureInfo("de-DE")));
        }

        [Fact]
        public void FormatCurrency_ZeroDecimalCurrencyHasNoDecimals()
        {
            var formatted = 500m.FormatCurrency("JPY", EnGb);
            Assert.EndsWith("500", formatted);
            Assert.DoesNotContain(".", formatted);
        }

        [Fact]
        public void FormatCurrency_ThreeDecimalCurrencyShowsThree()
            => Assert.Contains("1.235", 1.235m.FormatCurrency("KWD", EnGb));

        [Fact]
        public void FormatCurrency_UnknownCodeIsShownAfterTheAmount()
            => Assert.Equal("10.00 XYZ", 10m.FormatCurrency("XYZ", EnGb));

        [Fact]
        public void FormatCurrency_NoCodeIsAPlainAmount()
            => Assert.Equal("10.00", 10m.FormatCurrency(null, EnGb));

        [Fact]
        public void FormatCurrency_WorksInTheInvariantCulture()
            => Assert.Contains("1.99", 1.99m.FormatCurrency("GBP", CultureInfo.InvariantCulture));
    }
}
