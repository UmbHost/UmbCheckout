using System.Text.Json;
using UmbCheckout.Shared.Models;
using Xunit;

namespace UmbCheckout.Tests.Models
{
    /// <summary>The basket total used to be cached once and stored, so it went stale after a JSON round trip.</summary>
    public class BasketTests
    {
        [Fact]
        public void Total_IsTheSumOfTheLines()
            => Assert.Equal(25m, new Basket { LineItems = new List<LineItem> { new() { Price = 10m, Quantity = 2 }, new() { Price = 5m, Quantity = 1 } } }.Total);

        [Fact]
        public void Total_FollowsLinesAddedAfterAJsonRoundTrip()
        {
            var basket = new Basket { LineItems = new List<LineItem> { new() { Price = 10m, Quantity = 1 } } };
            _ = basket.Total;

            var restored = JsonSerializer.Deserialize<Basket>(JsonSerializer.Serialize(basket)).Required();
            restored.LineItems = restored.LineItems.Append(new LineItem { Price = 5m, Quantity = 2 }).ToList();

            Assert.Equal(20m, restored.Total);
        }

        [Fact]
        public void Total_FollowsLinesChangedAfterItWasRead()
        {
            var line = new LineItem { Price = 10m, Quantity = 1 };
            var basket = new Basket { LineItems = new List<LineItem> { line } };
            _ = basket.Total;

            line.Quantity = 3;

            Assert.Equal(30m, basket.Total);
        }

        [Fact]
        public void Total_IsNotSerialised()
            => Assert.DoesNotContain("\"Total\"", JsonSerializer.Serialize(new Basket { LineItems = new List<LineItem> { new() { Price = 1m, Quantity = 1 } } }));

        [Fact]
        public void Total_AStoredTotalIsIgnoredWhenRead()
        {
            // Baskets saved before this change carry their old (possibly stale) total.
            var restored = JsonSerializer.Deserialize<Basket>("{\"LineItems\":[{\"Price\":2,\"Quantity\":3}],\"Total\":99}").Required();

            Assert.Equal(6m, restored.Total);
        }

        [Fact]
        public void CurrencyCode_RoundTrips()
            => Assert.Equal("GBP", JsonSerializer.Deserialize<Basket>(JsonSerializer.Serialize(new Basket { CurrencyCode = "GBP" })).Required().CurrencyCode);

        [Fact]
        public void CurrencyCode_IsNullForABasketSavedBeforeBasketsWerePerCurrency()
            => Assert.Null(JsonSerializer.Deserialize<Basket>("{\"LineItems\":[]}").Required().CurrencyCode);
    }
}
