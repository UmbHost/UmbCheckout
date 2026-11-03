using System.Globalization;
using Moq;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Money;
using UmbCheckout.Stripe.Services;
using Umbraco.Cms.Core.Models.PublishedContent;
using Xunit;

namespace UmbCheckout.Tests.Helpers
{
    /// <summary>Checkout used to send Price * 100 for every currency (JPY charged 100x, KWD a tenth).</summary>
    public class StripeLineItemPriceTests
    {
        private static decimal D(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);

        [Theory]
        [InlineData("usd", "10.99", 1099)]
        [InlineData("gbp", "10.99", 1099)]
        [InlineData("jpy", "500", 500)]
        [InlineData("kwd", "1.235", 1235)]
        [InlineData("isk", "5", 500)]
        [InlineData("gbp", "1.995", 200)]   // unrounded price from a basket saved before the upgrade
        [InlineData("jpy", "10.5", 11)]     // likewise: rounded once by the single policy, never x100
        public void UnitAmount_IsInTheCurrencysMinorUnit(string currency, string price, long expected)
            => Assert.Equal(expected, StripeLineItemPrice.UnitAmount(D(price), currency));

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
            => Assert.Equal("gbp", StripeLineItemPrice.BasketCurrency(new[] { new LineItem { CurrencyCode = "GBP" }, new LineItem { CurrencyCode = "gbp" } }));

        [Fact]
        public void BasketCurrency_RejectsAnEmptyBasket()
            => Assert.Throws<InvalidOperationException>(() => StripeLineItemPrice.BasketCurrency(Array.Empty<LineItem>()));

        [Fact]
        public void BasketCurrency_RejectsMixedCurrencies()
            => Assert.Throws<InvalidOperationException>(() => StripeLineItemPrice.BasketCurrency(new[] { new LineItem { CurrencyCode = "GBP" }, new LineItem { CurrencyCode = "INR" } }));

        [Fact]
        public void BasketCurrency_RejectsAnEmptyCode()
            // The old "+" button posted no currency, so baskets saved before the upgrade can hold "".
            => Assert.Throws<ArgumentException>(() => StripeLineItemPrice.BasketCurrency(new[] { new LineItem { CurrencyCode = "" } }));

        [Fact]
        public void BasketCurrency_RejectsAnUnsupportedCode()
            => Assert.Throws<ArgumentException>(() => StripeLineItemPrice.BasketCurrency(new[] { new LineItem { CurrencyCode = "XYZ" } }));

        private static IPublishedContent Product(Guid key, string name)
        {
            var product = new Mock<IPublishedContent>();
            product.Setup(p => p.Key).Returns(key);
            product.Setup(p => p.Name).Returns(name);
            return product.Object;
        }

        [Fact]
        public void SessionLineItems_ChargeEachLineInTheBasketsMinorUnit()
        {
            Guid jacket = Guid.NewGuid(), ball = Guid.NewGuid();
            var products = new Dictionary<Guid, IPublishedContent> { [jacket] = Product(jacket, "Jacket node"), [ball] = Product(ball, "Ball node") };
            var lineItems = new[]
            {
                new LineItem { Key = jacket, Name = "Jacket", Price = 129.99m, CurrencyCode = "GBP", Quantity = 1 },
                // A basket saved before the upgrade: lower-case code, unrounded price.
                new LineItem { Key = ball, Price = 2.005m, CurrencyCode = "gbp", Quantity = 3 }
            };

            var built = StripeSessionService.BuildLineItems(lineItems, key => products.TryGetValue(key, out var p) ? p : null);

            Assert.Collection(built,
                first =>
                {
                    Assert.Equal(12999, first.Options.PriceData.UnitAmount);
                    Assert.Null(first.Options.PriceData.UnitAmountDecimal);
                    Assert.Equal("gbp", first.Options.PriceData.Currency);
                    Assert.Equal("Jacket", first.Options.PriceData.ProductData.Name);
                    Assert.Equal(1, first.Options.Quantity);
                    Assert.Same(products[jacket], first.Product);
                },
                second =>
                {
                    Assert.Equal(201, second.Options.PriceData.UnitAmount);
                    Assert.Equal("gbp", second.Options.PriceData.Currency);
                    Assert.Equal("Ball node", second.Options.PriceData.ProductData.Name);
                    Assert.Equal(3, second.Options.Quantity);
                });
        }

        [Fact]
        public void SessionLineItems_JpyIsNotMultipliedBy100()
        {
            var key = Guid.NewGuid();
            var built = StripeSessionService.BuildLineItems(new[] { new LineItem { Key = key, Price = 500m, CurrencyCode = "JPY", Quantity = 1 } }, _ => Product(key, "Teapot"));

            Assert.Equal(500, Assert.Single(built).Options.PriceData.UnitAmount);
        }

        [Fact]
        public void SessionLineItems_SkipLinesWhoseProductIsGone()
        {
            var key = Guid.NewGuid();
            var built = StripeSessionService.BuildLineItems(
                new[] { new LineItem { Key = key, Price = 1m, CurrencyCode = "GBP", Quantity = 1 }, new LineItem { Key = Guid.NewGuid(), Price = 2m, CurrencyCode = "GBP", Quantity = 1 } },
                k => k == key ? Product(key, "Kept") : null);

            Assert.Equal("Kept", Assert.Single(built).Options.PriceData.ProductData.Name);
        }

        [Fact]
        public void SessionLineItems_ALegacyLineWithoutACurrencyFailsRatherThanCharging()
            => Assert.Throws<ArgumentException>(() => StripeSessionService.BuildLineItems(
                new[] { new LineItem { Key = Guid.NewGuid(), Price = 10m, CurrencyCode = "", Quantity = 1 } },
                key => Product(key, "Legacy")));

        [Fact]
        public void SessionLineItems_MixedCurrenciesFail()
            => Assert.Throws<InvalidOperationException>(() => StripeSessionService.BuildLineItems(
                new[] { new LineItem { Key = Guid.NewGuid(), Price = 10m, CurrencyCode = "GBP", Quantity = 1 }, new LineItem { Key = Guid.NewGuid(), Price = 10m, CurrencyCode = "JPY", Quantity = 1 } },
                key => Product(key, "Mixed")));
    }
}
