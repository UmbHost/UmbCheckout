#pragma warning disable CS0618 // these tests cover the overloads without a token, which are obsolete but kept
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

        private StripeBasketPricingService Service()
        {
            var snapshot = new Mock<IPublishedSnapshot>();
            snapshot.Setup(s => s.Content).Returns(_cache.Object);
            var published = snapshot.Object;
            var accessor = new Mock<IPublishedSnapshotAccessor>();
            accessor.Setup(a => a.TryGetPublishedSnapshot(out published)).Returns(true);
            return new StripeBasketPricingService(accessor.Object, _pricing.Object, NullLogger<StripeBasketPricingService>.Instance);
        }

        private LineItem Line(decimal price, LineItemPrice? current, bool published = true)
        {
            var line = new LineItem { Key = Guid.NewGuid(), Name = "Jacket", Price = price, CurrencyCode = "GBP", Quantity = 1 };
            var product = Mock.Of<IPublishedContent>();
            _cache.Setup(c => c.GetById(line.Key)).Returns(published ? product : null);
            _pricing.Setup(p => p.GetPriceAsync(product, It.IsAny<CancellationToken>())).ReturnsAsync(current);
            return line;
        }

        private static Basket Basket(params LineItem[] lines) => new() { CurrencyCode = "GBP", LineItems = lines };

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1_000_000)]
        public async Task RepriceAsync_RemovesALegacyLineWithAQuantityStripeRefuses(long quantity)
        {
            // Saved before quantities were limited: Stripe would refuse the whole checkout (a 400, then a 500 here).
            var line = Line(10m, new LineItemPrice(10m, "GBP"));
            line.Quantity = quantity;
            var kept = Line(5m, new LineItemPrice(5m, "GBP"));

            var result = await Service().RepriceAsync(Basket(line, kept));

            Assert.True(Assert.Single(result.Changes).Removed);
            Assert.Equal(kept.Key, Assert.Single(result.Basket.LineItems).Key);
        }

        [Fact]
        public async Task RepriceAsync_RemovesLinesBeyondStripesLimit()
        {
            // Stripe Checkout takes at most 100 line items; a legacy basket may hold more.
            var lines = Enumerable.Range(0, Shared.Consts.MaxLineItems + 2).Select(_ => Line(1m, new LineItemPrice(1m, "GBP"))).ToArray();

            var result = await Service().RepriceAsync(Basket(lines));

            Assert.Equal(Shared.Consts.MaxLineItems, result.Basket.LineItems.Count());
            Assert.Equal(2, result.Changes.Count(change => change.Removed));
        }

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
        public async Task RepriceAsync_RepairsALegacyZeroPriceLine()
        {
            // Before the price was read on the server, an empty price field could put a line in the basket at 0.00.
            var result = await Service().RepriceAsync(Basket(Line(0m, new LineItemPrice(15m, "GBP"))));

            Assert.Equal(15m, Assert.Single(result.Basket.LineItems).Price);
            Assert.False(Assert.Single(result.Changes).Removed);
        }

        [Fact]
        public async Task RepriceAsync_RemovesALegacyZeroPriceLineWhoseProductStillHasNoPrice()
        {
            var result = await Service().RepriceAsync(Basket(Line(0m, null)));

            Assert.Empty(result.Basket.LineItems);
            Assert.True(Assert.Single(result.Changes).Removed);
        }

        [Fact]
        public async Task RepriceAsync_RepairsALegacyLineWithTooManyDecimals()
        {
            // Lines added before prices were rounded for their currency (10.005 GBP); the current price is rounded once.
            var result = await Service().RepriceAsync(Basket(Line(10.005m, new LineItemPrice(10.01m, "GBP"))));

            Assert.Equal(10.01m, Assert.Single(result.Basket.LineItems).Price);
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

        [Fact]
        public async Task RepriceAsync_KeepsTheOtherLines()
        {
            var unchanged = Line(5m, new LineItemPrice(5m, "GBP"));
            var result = await Service().RepriceAsync(Basket(unchanged, Line(10m, null, published: false)));

            Assert.Equal(unchanged.Key, Assert.Single(result.Basket.LineItems).Key);
        }
    }
}
