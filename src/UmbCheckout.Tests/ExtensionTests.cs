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

        [Fact]
        public void FormatCurrency()
        {
            const decimal amount = 1.99m;

            Assert.Equal("£1.99", amount.FormatCurrency("GBP"));

            // The "Z" currency code is unrecognised, so FormatCurrency falls back to
            // amount.ToString("F"), which is formatted using CultureInfo.CurrentCulture.
            // Pin the culture for the duration of this assertion so the test is
            // deterministic regardless of the machine/CI locale (e.g. "," vs "." decimal separator).
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                Assert.Equal("1.99", amount.FormatCurrency("Z"));
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }
    }
}
