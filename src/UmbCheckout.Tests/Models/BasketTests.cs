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
            => Assert.Equal(25m, new Basket { LineItems = [new LineItem { Price = 10m, Quantity = 2 }, new LineItem { Price = 5m, Quantity = 1 }] }.Total);

        [Fact]
        public void Total_FollowsLinesAddedAfterAJsonRoundTrip()
        {
            var basket = new Basket { LineItems = [new LineItem { Price = 10m, Quantity = 1 }] };
            _ = basket.Total;

            var restored = JsonSerializer.Deserialize<Basket>(JsonSerializer.Serialize(basket))!;
            restored.LineItems = restored.LineItems.Append(new LineItem { Price = 5m, Quantity = 2 }).ToList();

            Assert.Equal(20m, restored.Total);
        }

        [Fact]
        public void Total_IsNotSerialised()
            => Assert.DoesNotContain("\"Total\"", JsonSerializer.Serialize(new Basket { LineItems = [new LineItem { Price = 1m, Quantity = 1 }] }));

        [Fact]
        public void CurrencyCode_RoundTrips()
            => Assert.Equal("GBP", JsonSerializer.Deserialize<Basket>(JsonSerializer.Serialize(new Basket { CurrencyCode = "GBP" }))!.CurrencyCode);
    }
}
