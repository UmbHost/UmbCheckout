#pragma warning disable CS0618 // these tests cover the overloads without a token, which are obsolete but kept
using System.Globalization;
using Microsoft.Extensions.Logging;
using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Tests.Helpers;
using Umbraco.Cms.Core.Models.PublishedContent;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    /// <summary>The price and currency put in the basket come from the published product and the server, never the form.</summary>
    public class StripeLineItemPricingServiceTests
    {
        private readonly Mock<ICurrencyService> _currency = new();
        private readonly Mock<IBasketContext> _page = new();
        private readonly RecordingLogger<StripeLineItemPricingService> _logger = new();

        private StripeLineItemPricingService Service() => new(_currency.Object, _page.Object, _logger);

        /// <param name="price">The stored value; null leaves the property off the product.</param>
        /// <param name="hasValue">What Umbraco reports for the property (an empty Decimal field converts to 0m but has no value).</param>
        private IPublishedContent Product(object? price, string? currency, bool hasValue = true)
        {
            var product = new Mock<IPublishedContent>();
            product.Setup(p => p.Key).Returns(Guid.NewGuid());
            if (price != null)
            {
                var property = new Mock<IPublishedProperty>();
                property.Setup(p => p.GetValue(It.IsAny<string?>(), It.IsAny<string?>())).Returns(price);
                property.Setup(p => p.HasValue(It.IsAny<string?>(), It.IsAny<string?>())).Returns(hasValue);
                product.Setup(p => p.GetProperty(Consts.PropertyAlias.PriceAlias)).Returns(property.Object);
            }

            _currency.Setup(c => c.GetCurrencyForContentAsync(product.Object, It.IsAny<CancellationToken>())).ReturnsAsync(currency);
            return product.Object;
        }

        [Theory]
        [InlineData("10.99", "GBP", "10.99")]
        [InlineData("2.005", "GBP", "2.01")]
        [InlineData("1.995", "GBP", "2.00")]
        [InlineData("500", "JPY", "500")]
        [InlineData("10.5", "JPY", "11")]
        [InlineData("5.50", "ISK", "6")]
        [InlineData("1.2345", "KWD", "1.235")]
        public async Task GetPriceAsync_RoundsOnceForTheServersCurrency(string price, string currency, string expected)
        {
            var result = await Service().GetPriceAsync(Product(decimal.Parse(price, CultureInfo.InvariantCulture), currency));

            Assert.NotNull(result);
            Assert.Equal(decimal.Parse(expected, CultureInfo.InvariantCulture), result.Required().Price);
            Assert.Equal(currency, result.CurrencyCode);
        }

        [Fact]
        public async Task GetPriceAsync_AProductSoldInAnotherCurrencyThanThePageIsRefused()
        {
            // The basket shown on this page is in the page's currency: the product would go to a basket the customer never sees here.
            _page.Setup(p => p.GetCurrencyAsync(It.IsAny<CancellationToken>())).ReturnsAsync("EUR");

            Assert.Null(await Service().GetPriceAsync(Product(10m, "GBP")));
            Assert.Equal(1, _logger.Count(LogLevel.Warning));
        }

        [Fact]
        public async Task GetPriceAsync_AProductInThePagesCurrencyIsPriced()
        {
            _page.Setup(p => p.GetCurrencyAsync(It.IsAny<CancellationToken>())).ReturnsAsync("gbp");

            Assert.Equal(new LineItemPriceShape(10m, "GBP"), Shape(await Service().GetPriceAsync(Product(10m, "GBP"))));
        }

        [Fact]
        public async Task GetPriceAsync_WithNoPageCurrencyThePriceStands()
            => Assert.NotNull(await Service().GetPriceAsync(Product(10m, "GBP")));

        private sealed record LineItemPriceShape(decimal Price, string CurrencyCode);

        private static LineItemPriceShape? Shape(UmbCheckout.Stripe.Interfaces.LineItemPrice? price) => price == null ? null : new(price.Price, price.CurrencyCode);

        [Fact]
        public async Task GetPriceAsync_WarnsWhenItRounds()
        {
            await Service().GetPriceAsync(Product(2.005m, "GBP"));
            Assert.Equal(1, _logger.Count(LogLevel.Warning));
        }

        [Fact]
        public async Task GetPriceAsync_TheCurrencyIsUpperCase()
            => Assert.Equal("GBP", (await Service().GetPriceAsync(Product(1m, "gbp"))).Required().CurrencyCode);

        [Fact]
        public async Task GetPriceAsync_ATypedZeroIsAFreeProduct()
            => Assert.Equal(0m, (await Service().GetPriceAsync(Product(0m, "GBP"))).Required().Price);

        [Fact]
        public async Task GetPriceAsync_EmptyPriceField_ReturnsNull()
        {
            // An empty Umbraco.Decimal field converts to 0m: it used to be sold at 0.00.
            Assert.Null(await Service().GetPriceAsync(Product(0m, "GBP", hasValue: false)));
            Assert.Equal(1, _logger.Count(LogLevel.Error));
        }

        [Fact]
        public async Task GetPriceAsync_NoPriceProperty_ReturnsNull()
        {
            // A variant without its own price used to be added at 0.00.
            Assert.Null(await Service().GetPriceAsync(Product(null, "GBP")));
            Assert.Equal(1, _logger.Count(LogLevel.Error));
        }

        [Fact]
        public async Task GetPriceAsync_LogsWithoutAssumingABasketAdd()
        {
            // Checkout re-prices through this too: "it was not added to the basket" misled when nothing was being added.
            await Service().GetPriceAsync(Product(null, "GBP"));

            Assert.DoesNotContain("added to the basket", Assert.Single(_logger.Entries).Message);
        }

        [Fact]
        public async Task GetPriceAsync_NegativePrice_ReturnsNull()
            => Assert.Null(await Service().GetPriceAsync(Product(-1m, "GBP")));

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("G1P")]
        [InlineData("XYZ")]
        [InlineData("GPB")]
        public async Task GetPriceAsync_NoSupportedCurrency_ReturnsNull(string? currency)
        {
            Assert.Null(await Service().GetPriceAsync(Product(10m, currency)));
            Assert.Equal(1, _logger.Count(LogLevel.Error));
        }

        [Theory]
        [InlineData(10)]
        [InlineData(10L)]
        [InlineData(10.0)]
        [InlineData("10")]
        public async Task GetPriceAsync_ReadsOtherNumericEditors(object price)
            => Assert.Equal(10m, (await Service().GetPriceAsync(Product(price, "GBP"))).Required().Price);
    }
}
