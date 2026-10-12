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

        // Umbraco 17.6+ cache-busts /App_Plugins URLs with the manifest version (?umb__rnd=<version>), so pre-releases
        // must keep their suffix or every beta would share one cache key. Build metadata (+sha) is not part of the version.
        [Theory]
        [InlineData("2.0.0-beta.3+Branch.develop.Sha.abc123", "2.0.0-beta.3")]
        [InlineData("2.0.0+abc123", "2.0.0")]
        [InlineData("2.0.1", "2.0.1")]
        public void PackageVersion_KeepsThePrereleaseAndDropsBuildMetadata(string informational, string expected)
            => Assert.Equal(expected, UmbCheckout.Shared.UmbCheckoutVersion.ToPackageVersion(informational));

        [Fact]
        public async Task CoreManifest_UsesThePackageVersion()
        {
            var manifest = Assert.Single(await new UmbCheckoutReader().ReadPackageManifestsAsync());

            Assert.Equal(UmbCheckout.Shared.UmbCheckoutVersion.PackageVersion, manifest.Version);
        }
    }
}
