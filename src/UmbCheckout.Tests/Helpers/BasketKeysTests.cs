using UmbCheckout.Shared.Helpers;
using Xunit;

namespace UmbCheckout.Tests.Helpers
{
    /// <summary>One basket per site root culture and currency: each has its own session key and cookie.</summary>
    public class BasketKeysTests
    {
        [Fact]
        public void Session_IsPerCultureAndCurrency()
            => Assert.NotEqual(BasketKeys.Session("en-GB", "GBP"), BasketKeys.Session("en-GB", "USD"));

        [Fact]
        public void Session_IsPerCulture()
            => Assert.NotEqual(BasketKeys.Session("en-GB", "EUR"), BasketKeys.Session("fr-FR", "EUR"));

        [Fact]
        public void Session_NormalisesTheCurrency()
            => Assert.Equal(BasketKeys.Session("en-GB", "GBP"), BasketKeys.Session("en-GB", " gbp "));

        [Fact]
        public void Session_WithNoCurrencyIsTheLegacyKey()
            // A site with no currency configured keeps its basket where it always was.
            => Assert.Equal(BasketKeys.LegacySession("en-GB"), BasketKeys.Session("en-GB", null));

        [Fact]
        public void Cookie_WithNoCurrencyIsTheLegacyName()
            => Assert.Equal(BasketKeys.LegacyCookie("en-GB"), BasketKeys.Cookie("en-GB", " "));

        [Fact]
        public void Cookie_DiffersFromSession()
            => Assert.NotEqual(BasketKeys.Session("en-GB", "GBP"), BasketKeys.Cookie("en-GB", "GBP"));

        [Fact]
        public void Legacy_KeysMatchThePreviousFormat()
        {
            Assert.Equal("UmbCheckoutSessionen-GB", BasketKeys.LegacySession("en-GB"));
            Assert.Equal("UmbCheckoutSession_Basketen-GB", BasketKeys.LegacyCookie("en-GB"));
            Assert.Equal("UmbCheckoutSession", BasketKeys.LegacySession(null));
        }
    }
}
