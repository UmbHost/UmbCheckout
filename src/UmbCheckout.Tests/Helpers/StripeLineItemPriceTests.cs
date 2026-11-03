using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Money;
using Xunit;

namespace UmbCheckout.Tests.Helpers
{
    /// <summary>Checkout used to send Price * 100 for every currency (JPY charged 100x, KWD a tenth).</summary>
    public class StripeLineItemPriceTests
    {
        [Theory]
        [InlineData("usd", "10.99", 1099)]
        [InlineData("jpy", "500", 500)]
        [InlineData("kwd", "1.235", 1235)]
        [InlineData("isk", "5", 500)]
        [InlineData("gbp", "1.995", 200)]   // unrounded price from a basket saved before the upgrade
        public void UnitAmount_IsInTheCurrencysMinorUnit(string currency, string price, long expected)
            => Assert.Equal(expected, StripeLineItemPrice.UnitAmount(decimal.Parse(price, System.Globalization.CultureInfo.InvariantCulture), currency));

        [Fact]
        public void PriceData_UsesUnitAmountAndALowercaseCurrency()
        {
            var priceData = StripeLineItemPrice.PriceData(new LineItem { Name = "Jacket", Price = 500m, CurrencyCode = "JPY" }, "jpy", "Product node");

            Assert.Equal(500, priceData.UnitAmount);
            Assert.Null(priceData.UnitAmountDecimal);
            Assert.Equal("jpy", priceData.Currency);
            Assert.Equal("Jacket", priceData.ProductData.Name);
        }

        [Fact]
        public void PriceData_FallsBackToTheProductName()
            => Assert.Equal("Product node", StripeLineItemPrice.PriceData(new LineItem { Price = 1m }, "gbp", "Product node").ProductData.Name);

        [Fact]
        public void BasketCurrency_IsTheLinesSharedCurrencyInLowercase()
            => Assert.Equal("gbp", StripeLineItemPrice.BasketCurrency([new LineItem { CurrencyCode = "GBP" }, new LineItem { CurrencyCode = "gbp" }]));

        [Fact]
        public void BasketCurrency_RejectsMixedCurrencies()
            => Assert.Throws<InvalidOperationException>(() => StripeLineItemPrice.BasketCurrency([new LineItem { CurrencyCode = "GBP" }, new LineItem { CurrencyCode = "INR" }]));

        [Fact]
        public void BasketCurrency_RejectsAnEmptyCode()
            // The old "+" button posted no currency, so baskets saved before the upgrade can hold "".
            => Assert.Throws<ArgumentException>(() => StripeLineItemPrice.BasketCurrency([new LineItem { CurrencyCode = "" }]));
    }
}
