using UmbCheckout.Shared;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Infrastructure.Manifest;

namespace UmbCheckout.Backoffice.Manifests
{
    /// <summary>
    /// Registers the UmbCheckout package manifest, which loads the backoffice extensions bundle
    /// </summary>
    internal class UmbCheckoutPackageManifestReader : IPackageManifestReader
    {
        public Task<IEnumerable<PackageManifest>> ReadPackageManifestsAsync()
        {
            IEnumerable<PackageManifest> manifests =
            [
                new()
                {
                    Id = Consts.PackageName,
                    Name = Consts.PackageName,
                    Version = UmbCheckoutVersion.Version.ToString(3),
                    AllowTelemetry = true,
                    Extensions =
                    [
                        new
                        {
                            type = "bundle",
                            alias = "UmbCheckout.Bundle",
                            name = "UmbCheckout Bundle",
                            js = "/App_Plugins/UmbCheckout/umbcheckout.js"
                        }
                    ]
                }
            ];

            return Task.FromResult(manifests);
        }
    }
}
