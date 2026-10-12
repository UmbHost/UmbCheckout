using Microsoft.Extensions.Logging;
using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Models;
using UmbCheckout.Shared;
using UmbCheckout.Stripe.Services;
using Umbraco.Cms.Core.Models.PublishedContent;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    /// <summary>The price and currency put in the basket come from the published product and the server, never the form.</summary>
    public class StripeLineItemPricingServiceTests
    {
        private readonly Mock<ICurrencyService> _currency = new();
        private readonly RecordingLogger<StripeLineItemPricingService> _logger = new();
        private readonly Mock<IBasketContext> _basketContext = new();

        private StripeLineItemPricingService Service() => new(_currency.Object, _basketContext.Object, _logger);

        /// <summary>Moq cannot proxy ILogger&lt;T&gt; for an internal T, so the log levels are recorded here.</summary>
        private sealed class RecordingLogger<T> : ILogger<T>
        {
            public List<LogLevel> Levels { get; } = [];

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
                => Levels.Add(logLevel);
        }

        private IPublishedContent Product(object? price, string? currency, bool hasValue = true)
        {
            var product = new Mock<IPublishedContent>();
            product.Setup(p => p.Key).Returns(Guid.NewGuid());
            if (price != null)
            {
                var property = new Mock<IPublishedProperty>();
                property.Setup(p => p.HasValue(It.IsAny<string?>(), It.IsAny<string?>())).Returns(hasValue);
                property.Setup(p => p.GetValue(It.IsAny<string?>(), It.IsAny<string?>())).Returns(price);
                product.Setup(p => p.GetProperty(Consts.PropertyAlias.PriceAlias)).Returns(property.Object);
            }

            _currency.Setup(c => c.GetCurrencyForContentAsync(product.Object)).ReturnsAsync(currency);
            return product.Object;
        }

        private void VerifyLogged(LogLevel level)
            => Assert.Equal(level, Assert.Single(_logger.Levels));

        [Theory]
        [InlineData("10.99", "GBP", "10.99")]
        [InlineData("500.5", "JPY", "501")]
        [InlineData("5.50", "ISK", "6")]
        [InlineData("1.995", "GBP", "2.00")]
        public async Task GetPriceAsync_RoundsOnceForTheServersCurrency(string price, string currency, string expected)
        {
            var result = await Service().GetPriceAsync(Product(decimal.Parse(price, System.Globalization.CultureInfo.InvariantCulture), currency));

            Assert.NotNull(result);
            Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture), result.Price);
            Assert.Equal(currency, result.CurrencyCode);
        }

        [Fact]
        public async Task GetPriceAsync_WarnsWhenItRounds()
        {
            await Service().GetPriceAsync(Product(1.995m, "GBP"));
            VerifyLogged(LogLevel.Warning);
        }

        [Fact]
        public async Task GetPriceAsync_NoPrice_ReturnsNull()
        {
            // A variant without its own price used to be added at 0.00.
            Assert.Null(await Service().GetPriceAsync(Product(null, "GBP")));
            VerifyLogged(LogLevel.Error);
        }

        [Fact]
        public async Task GetPriceAsync_EmptyPriceField_ReturnsNull()
        {
            // Umbraco's Decimal converter turns an empty field into 0m; a variant left without a price must not sell at 0.00.
            Assert.Null(await Service().GetPriceAsync(Product(0m, "GBP", hasValue: false)));
            VerifyLogged(LogLevel.Error);
        }

        [Fact]
        public async Task GetPriceAsync_ATypedZeroIsAFreeProduct()
            => Assert.Equal(0m, (await Service().GetPriceAsync(Product(0m, "GBP")))?.Price);

        [Fact]
        public async Task GetPriceAsync_NegativePrice_ReturnsNull()
            => Assert.Null(await Service().GetPriceAsync(Product(-1m, "GBP")));

        [Theory]
        [InlineData(null)]
        [InlineData("G1P")]
        [InlineData("XYZ")]
        public async Task GetPriceAsync_NoValidCurrency_ReturnsNull(string? currency)
            => Assert.Null(await Service().GetPriceAsync(Product(10m, currency)));

        [Fact]
        public async Task GetPriceAsync_AProductSoldInAnotherCurrencyThanThePageIsRefused()
        {
            // e.g. a shared product library or a listing from another site: its line would go to a basket this page never shows.
            _basketContext.Setup(b => b.GetCurrencyAsync()).ReturnsAsync("USD");

            Assert.Null(await Service().GetPriceAsync(Product(10m, "GBP")));
            VerifyLogged(LogLevel.Warning);
        }

        [Fact]
        public async Task GetPriceAsync_AProductInThePagesCurrencyIsPriced()
        {
            _basketContext.Setup(b => b.GetCurrencyAsync()).ReturnsAsync("gbp");

            Assert.Equal(10m, (await Service().GetPriceAsync(Product(10m, "GBP")))?.Price);
        }

        [Fact]
        public void BasketAdd_HasNoCurrencyField()
            // The currency used to be posted from a hidden field: £399 posted as "inr" was charged ₹399.
            => Assert.Null(typeof(BasketAdd).GetProperty("CurrencyCode"));
    }
}
