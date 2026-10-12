using UmbCheckout.Stripe.Helpers;
using Xunit;

namespace UmbCheckout.Tests.Helpers
{
    /// <summary>The old merge threw on duplicate keys, never added the product's own values and changed the basket's line.</summary>
    public class CheckoutMetadataTests
    {
        private static readonly Guid ProductKey = Guid.NewGuid();

        [Fact]
        public void ForLine_AddsTheNodeKey()
            => Assert.Equal(ProductKey.ToString(), CheckoutMetadata.ForLine(new Dictionary<string, string>(), null, ProductKey)["nodeKey"]);

        [Fact]
        public void ForLine_AddsTheProductsMetadata()
            => Assert.Equal("blue", CheckoutMetadata.ForLine(new Dictionary<string, string>(), new Dictionary<string, string> { ["colour"] = "blue" }, ProductKey)["colour"]);

        [Fact]
        public void ForLine_TheLinesOwnValueWinsWithoutThrowing()
            => Assert.Equal("red", CheckoutMetadata.ForLine(new Dictionary<string, string> { ["colour"] = "red" }, new Dictionary<string, string> { ["colour"] = "blue" }, ProductKey)["colour"]);

        [Fact]
        public void ForLine_LeavesTheLinesMetadataUnchanged()
        {
            var line = new Dictionary<string, string> { ["size"] = "L" };
            CheckoutMetadata.ForLine(line, new Dictionary<string, string> { ["colour"] = "blue" }, ProductKey);

            Assert.Equal(["size"], line.Keys);
        }
    }
}
