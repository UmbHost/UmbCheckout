using UmbCheckout.Shared.Helpers;
using UmbCheckout.Shared.Models;
using Xunit;

namespace UmbCheckout.Tests.Helpers
{
    /// <summary>Baskets saved before they were per currency are split into one basket per line currency.</summary>
    public class BasketSplitterTests
    {
        private static Basket Legacy(params LineItem[] lines) => new()
        {
            Id = "42", SessionId = "s1", SiteRootCulture = "en-GB", CustomerReferenceId = "ref",
            MetaData = new Dictionary<string, string> { ["a"] = "b" }, LineItems = lines
        };

        [Fact]
        public void ByCurrency_GivesOneBasketPerCurrency()
        {
            var split = BasketSplitter.ByCurrency(Legacy(
                new LineItem { Key = Guid.NewGuid(), CurrencyCode = "GBP" },
                new LineItem { Key = Guid.NewGuid(), CurrencyCode = "USD" },
                new LineItem { Key = Guid.NewGuid(), CurrencyCode = " gbp " }));

            Assert.Equal(new[] { "GBP", "USD" }, split.Baskets.Select(b => b.CurrencyCode).OrderBy(c => c));
            Assert.Equal(2, split.Baskets.Single(b => b.CurrencyCode == "GBP").LineItems.Count());
        }

        [Fact]
        public void ByCurrency_DropsLinesWithNoCurrency()
        {
            var noCurrency = new LineItem { Key = Guid.NewGuid(), CurrencyCode = "" };
            var split = BasketSplitter.ByCurrency(Legacy(noCurrency, new LineItem { Key = Guid.NewGuid(), CurrencyCode = "GBP" }));

            Assert.Equal(noCurrency.Key, Assert.Single(split.Dropped).Key);
            Assert.Single(split.Baskets);
        }

        [Fact]
        public void ByCurrency_KeepsTheBasketDetailsButNotItsId()
        {
            var basket = Assert.Single(BasketSplitter.ByCurrency(Legacy(new LineItem { CurrencyCode = "GBP" })).Baskets);

            Assert.Equal(("s1", "en-GB", "ref", "b"), (basket.SessionId, basket.SiteRootCulture, basket.CustomerReferenceId, basket.MetaData["a"]));
            Assert.Equal(string.Empty, basket.Id);
        }

        [Fact]
        public void ByCurrency_EachBasketHasItsOwnMetadata()
        {
            var split = BasketSplitter.ByCurrency(Legacy(new LineItem { CurrencyCode = "GBP" }, new LineItem { CurrencyCode = "USD" }));

            split.Baskets[0].MetaData["a"] = "changed";

            Assert.Equal("b", split.Baskets[1].MetaData["a"]);
        }

        [Fact]
        public void ByCurrency_AnEmptyBasketGivesNone()
            => Assert.Empty(BasketSplitter.ByCurrency(Legacy()).Baskets);
    }
}
