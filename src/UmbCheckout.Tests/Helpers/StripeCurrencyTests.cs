using UmbCheckout.Stripe.Money;
using Xunit;

namespace UmbCheckout.Tests.Helpers
{
    /// <summary>
    /// Stripe amounts are integers in the currency's minor unit (https://docs.stripe.com/currencies#minor-units).
    /// Cases are Stripe's own documented examples.
    /// </summary>
    public class StripeCurrencyTests
    {
        [Theory]
        [InlineData("usd", 1099, "10.99")]   // "enter 1099 to charge 10.99 USD"
        [InlineData("gbp", 30, "0.30")]
        [InlineData("jpy", 500, "500")]      // zero-decimal: "to charge 500 JPY, provide an amount value of 500"
        [InlineData("krw", 50, "50")]
        [InlineData("xof", 1000, "1000")]
        [InlineData("isk", 500, "5")]        // special case: ISK is represented as two-decimal ("5 ISK ... 500")
        [InlineData("ugx", 500, "5")]        // special case: UGX is represented as two-decimal ("5 UGX ... 500")
        [InlineData("huf", 1045, "10.45")]   // HUF charges are two-decimal
        [InlineData("kwd", 1230, "1.230")]   // three-decimal: minor unit is the fils (1/1000)
        [InlineData("JPY", 500, "500")]      // codes are case-insensitive
        public void ToMajorUnits_UsesTheCurrencysStripeMinorUnit(string currency, long minorUnits, string expected)
            => Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture), StripeCurrency.ToMajorUnits(minorUnits, currency));

        [Theory]
        [InlineData("usd", "10.99", 1099)]
        [InlineData("jpy", "500", 500)]
        [InlineData("isk", "5", 500)]
        [InlineData("kwd", "1.23", 1230)]
        public void ToMinorUnits_IsTheInverse(string currency, string major, long expected)
            => Assert.Equal(expected, StripeCurrency.ToMinorUnits(decimal.Parse(major, System.Globalization.CultureInfo.InvariantCulture), currency));

        [Fact]
        public void ToMinorUnits_RefusesFractionsSmallerThanTheMinorUnit()
            // Silently rounding money is a bug; callers must round deliberately before converting.
            => Assert.Throws<ArgumentException>(() => StripeCurrency.ToMinorUnits(10.5m, "jpy"));

        [Fact]
        public void MinorUnitDigits_DefaultsToTwo()
            => Assert.Equal(2, StripeCurrency.MinorUnitDigits("eur"));

        [Theory]
        [InlineData("gbp", "1.995", "2.00")]   // half away from zero (commercial rounding)
        [InlineData("gbp", "1.994", "1.99")]
        [InlineData("usd", "10", "10")]
        [InlineData("jpy", "500.5", "501")]    // zero-decimal
        [InlineData("isk", "5.50", "6")]       // whole units only ("You can't charge fractions of ISK")
        [InlineData("ugx", "5.49", "5")]
        [InlineData("kwd", "1.2345", "1.235")] // three-decimal
        public void Round_RoundsToTheCurrencysChargeableDigits(string currency, string amount, string expected)
            => Assert.Equal(
                decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture),
                StripeCurrency.Round(decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture), currency));

        [Theory]
        [InlineData("isk")]
        [InlineData("ugx")]
        public void ToMinorUnits_RefusesFractionsOfWholeUnitCurrencies(string currency)
            // 5.50 ISK would be sent as 550, which Stripe cannot charge.
            => Assert.Throws<ArgumentException>(() => StripeCurrency.ToMinorUnits(5.50m, currency));

        [Theory]
        [InlineData("GBP", "gbp")]
        [InlineData("jpy", "jpy")]
        public void ApiCode_IsLowercase(string currency, string expected)
            => Assert.Equal(expected, StripeCurrency.ApiCode(currency));

        [Theory]
        [InlineData("")]
        [InlineData("GB")]
        [InlineData("G1P")]
        [InlineData("GBPX")]
        [InlineData("£££")]
        public void ApiCode_RejectsInvalidCodes(string currency)
            => Assert.Throws<ArgumentException>(() => StripeCurrency.ApiCode(currency));

        [Fact]
        public void IsCurrencyCode_RejectsNull()
            => Assert.False(StripeCurrency.IsCurrencyCode(null));
    }
}
