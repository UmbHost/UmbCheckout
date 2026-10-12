using System.Text.Json;
using UmbCheckout;
using Xunit;

namespace UmbCheckout.Tests
{
    public class ManifestReaderTests
    {
        [Fact]
        public async Task CoreManifest_RegistersBundleAndImportmap()
        {
            var manifest = Assert.Single(await new UmbCheckoutReader().ReadPackageManifestsAsync());

            var bundle = JsonSerializer.SerializeToElement(Assert.Single(manifest.Extensions));
            Assert.Equal("bundle", bundle.GetProperty("type").GetString());
            Assert.Equal("/App_Plugins/UmbCheckout/umbcheckout.js", bundle.GetProperty("js").GetString());
            Assert.Equal("/App_Plugins/UmbCheckout/umbcheckout-core.js", manifest.Importmap!.Imports["@umbcheckout/core"]);
        }
    }
}
