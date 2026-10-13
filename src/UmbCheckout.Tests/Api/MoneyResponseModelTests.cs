using UmbCheckout.Stripe.Models.Api;
using Xunit;

namespace UmbCheckout.Tests.Api
{
    /// <summary>The money object the backoffice APIs add next to each Stripe amount: converted once, on the server.</summary>
    public class MoneyResponseModelTests
    {
        [Theory]
        [InlineData(500, "jpy", "500")]     // zero-decimal
        [InlineData(495, "gbp", "4.95")]    // two-decimal
        [InlineData(1234, "kwd", "1.234")]  // three-decimal
        [InlineData(500, "isk", "5")]       // whole units, sent ×100
        [InlineData(495, "GBP", "4.95")]    // upper case from a caller
        public void Amount_IsInMajorUnits(long minorUnits, string currency, string expected)
        {
            var money = MoneyResponseModel.From(minorUnits, currency);

            Assert.Equal(minorUnits, money.MinorUnits);
            Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture), money.Amount);
            Assert.Equal(currency, money.Currency);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void NoCurrency_DoesNotThrow_AndLeavesTheAmountUnconverted(string? currency)
        {
            // Without a currency the major amount can't be known: it is null (never a guessed /100), and the minor units are kept.
            var money = MoneyResponseModel.From(495, currency);

            Assert.Equal(495, money.MinorUnits);
            Assert.Null(money.Amount);
            Assert.Equal(string.Empty, money.Currency);
        }
    }
}
