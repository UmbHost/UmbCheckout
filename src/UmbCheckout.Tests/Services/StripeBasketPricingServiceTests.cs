using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Services;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    /// <summary>Checkout charges the current published price; the customer is told what changed.</summary>
    public class StripeBasketPricingServiceTests
    {
        private readonly Mock<IPublishedContentCache> _cache = new();
        private readonly Mock<IStripeLineItemPricingService> _pricing = new();

        private StripeBasketPricingService Service() => new(_cache.Object, _pricing.Object, NullLogger<StripeBasketPricingService>.Instance);

        private LineItem Line(decimal price, LineItemPrice? current, bool published = true)
        {
            var line = new LineItem { Key = Guid.NewGuid(), Name = "Jacket", Price = price, CurrencyCode = "GBP", Quantity = 1 };
            var product = Mock.Of<IPublishedContent>();
            _cache.Setup(c => c.GetByIdAsync(line.Key, It.IsAny<bool?>())).ReturnsAsync(published ? product : null);
            _pricing.Setup(p => p.GetPriceAsync(product)).ReturnsAsync(current);
            return line;
        }

        private static Basket Basket(params LineItem[] lines) => new() { CurrencyCode = "GBP", LineItems = lines };

        [Fact]
        public async Task RepriceAsync_LeavesAnUnchangedBasketAlone()
        {
            var result = await Service().RepriceAsync(Basket(Line(10m, new LineItemPrice(10m, "GBP"))));

            Assert.Empty(result.Changes);
            Assert.Single(result.Basket.LineItems);
        }

        [Fact]
        public async Task RepriceAsync_UpdatesAChangedPrice()
        {
            var result = await Service().RepriceAsync(Basket(Line(10m, new LineItemPrice(12.50m, "GBP"))));

            var change = Assert.Single(result.Changes);
            Assert.Equal((10m, 12.50m, false), (change.OldPrice, change.NewPrice!.Value, change.Removed));
            Assert.Equal(12.50m, Assert.Single(result.Basket.LineItems).Price);
        }

        [Fact]
        public async Task RepriceAsync_RemovesAnUnpublishedProduct()
        {
            var result = await Service().RepriceAsync(Basket(Line(10m, null, published: false)));

            Assert.True(Assert.Single(result.Changes).Removed);
            Assert.Empty(result.Basket.LineItems);
        }

        [Fact]
        public async Task RepriceAsync_RemovesAProductThatCanNoLongerBePriced()
            => Assert.True(Assert.Single((await Service().RepriceAsync(Basket(Line(10m, null)))).Changes).Removed);

        [Fact]
        public async Task RepriceAsync_RemovesAProductNowSoldInAnotherCurrency()
            => Assert.True(Assert.Single((await Service().RepriceAsync(Basket(Line(10m, new LineItemPrice(10m, "USD"))))).Changes).Removed);
    }
}
