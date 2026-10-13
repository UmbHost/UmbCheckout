using UmbCheckout.Stripe.Helpers;
using Xunit;

namespace UmbCheckout.Tests.Helpers
{
    /// <summary>The old merge threw on any key the line and the product (or "nodeKey") shared, so checkout failed; it also never added the product's own values.</summary>
    public class CheckoutMetadataTests
    {
        private static readonly Guid ProductKey = Guid.NewGuid();

        private static Dictionary<string, string>? ForLine(Dictionary<string, string> line, Dictionary<string, string>? product)
        {
            Dictionary<string, string>? metaData = null;
            var exception = Record.Exception(() => metaData = CheckoutMetadata.ForLine(line, product, ProductKey));
            Assert.Null(exception);
            return metaData;
        }

        [Fact]
        public void ForLine_AddsTheNodeKey()
            => Assert.Equal(ProductKey.ToString(), ForLine(new Dictionary<string, string>(), null)!.GetValueOrDefault("nodeKey"));

        [Fact]
        public void ForLine_AddsTheProductsMetadata()
            => Assert.Equal("blue", ForLine(new Dictionary<string, string> { ["size"] = "L" }, new Dictionary<string, string> { ["colour"] = "blue" })!.GetValueOrDefault("colour"));

        [Fact]
        public void ForLine_TheLinesOwnValueWinsWithoutThrowing()
            => Assert.Equal("red", ForLine(new Dictionary<string, string> { ["colour"] = "red" }, new Dictionary<string, string> { ["colour"] = "blue" })!.GetValueOrDefault("colour"));

        [Fact]
        public void ForLine_ALineThatAlreadyHasANodeKeyKeepsItWithoutThrowing()
            => Assert.Equal("saved", ForLine(new Dictionary<string, string> { ["nodeKey"] = "saved" }, null)!.GetValueOrDefault("nodeKey"));

        [Fact]
        public void ForLine_LeavesTheLinesMetadataUnchanged()
        {
            var line = new Dictionary<string, string> { ["size"] = "L" };
            ForLine(line, new Dictionary<string, string> { ["colour"] = "blue" });

            Assert.Equal(new[] { "size" }, line.Keys);
        }
    }
}
