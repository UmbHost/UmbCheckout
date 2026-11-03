using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Services;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    /// <summary>The session records which basket it pays for, with the currency trimmed and in upper case.</summary>
    public class StripeSessionBasketMetadataTests
    {
        [Theory]
        [InlineData("GBP", "stripe-currency", "GBP")]
        [InlineData(" gbp ", "usd", "GBP")]
        [InlineData(null, " usd ", "USD")]
        [InlineData("  ", "eur", "EUR")]
        public void The_currency_is_trimmed_and_upper_case(string? basketCurrency, string sessionCurrency, string expected)
        {
            var metadata = StripeSessionService.BasketMetadata(new Basket { SiteRootCulture = "en-GB", CurrencyCode = basketCurrency }, sessionCurrency);

            Assert.Equal(expected, metadata["umbcheckout_currency"]);
            Assert.Equal("en-GB", metadata["umbcheckout_culture"]);
        }

        [Fact]
        public void A_basket_without_a_culture_records_an_empty_one()
            => Assert.Equal(string.Empty, StripeSessionService.BasketMetadata(new Basket(), "gbp")["umbcheckout_culture"]);
    }
}
