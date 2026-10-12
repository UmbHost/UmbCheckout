using Microsoft.Extensions.DependencyInjection;
using UmbCheckout.Shared;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Infrastructure.Manifest;

namespace UmbCheckout
{
    public class UmbCheckoutManifest : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<IPackageManifestReader, UmbCheckoutReader>();
        }
    }

    internal sealed class UmbCheckoutReader : IPackageManifestReader
    {
        public Task<IEnumerable<PackageManifest>> ReadPackageManifestsAsync()
        {
            IEnumerable<PackageManifest> manifests =
            [
                new PackageManifest
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
                    ],
                    Importmap = new PackageManifestImportmap
                    {
                        Imports = new Dictionary<string, string> { ["@umbcheckout/core"] = "/App_Plugins/UmbCheckout/umbcheckout-core.js" }
                    }
                }
            ];
            return Task.FromResult(manifests);
        }
    }
}
