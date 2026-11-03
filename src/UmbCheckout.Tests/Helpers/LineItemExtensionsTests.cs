using UmbCheckout.Shared.Extensions;
using UmbCheckout.Shared.Models;
using Xunit;

namespace UmbCheckout.Tests.Helpers
{
    /// <summary>The basket's own currency drives the subtotal display and which lines may join it.</summary>
    public class LineItemExtensionsTests
    {
        [Fact]
        public void SingleCurrency_IsTheLinesSharedCurrency()
            => Assert.Equal("GBP", new[] { new LineItem { CurrencyCode = "GBP" }, new LineItem { CurrencyCode = "gbp" } }.SingleCurrency());

        [Fact]
        public void SingleCurrency_IsNullForAnEmptyBasket()
            => Assert.Null(Array.Empty<LineItem>().SingleCurrency());

        [Fact]
        public void SingleCurrency_IsNullWhenLinesDisagree()
            => Assert.Null(new[] { new LineItem { CurrencyCode = "GBP" }, new LineItem { CurrencyCode = "USD" } }.SingleCurrency());

        [Fact]
        public void SingleCurrency_IsNullForALegacyLineWithoutACurrency()
            => Assert.Null(new[] { new LineItem { CurrencyCode = "" } }.SingleCurrency());

        [Fact]
        public void SingleCurrency_IsNullWhenALegacyLineSitsBesideAPricedOne()
            => Assert.Null(new[] { new LineItem { CurrencyCode = "GBP" }, new LineItem { CurrencyCode = null } }.SingleCurrency());
    }
}
