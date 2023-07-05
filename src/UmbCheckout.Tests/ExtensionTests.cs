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
            Assert.Equal("1.99", amount.FormatCurrency("Z"));
        }
    }
}
