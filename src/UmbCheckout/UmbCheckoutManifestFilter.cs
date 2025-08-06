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
            builder.Services.AddSingleton<IPackageManifestReader, UmbCheckoutManifestFilter>();
        }
    }

    //public class UmbCheckoutManifestFilter : IManifestFilter
    //{
    //    public void Filter(List<PackageManifest> manifests)
    //    {
    //        manifests.Add(new PackageManifest
    //        {
    //            PackageName = Consts.PackageName,
    //            Version = UmbCheckoutVersion.Version.ToString(3),
    //            AllowPackageTelemetry = true,
    //            BundleOptions = BundleOptions.None,
    //            Scripts = new []
    //            {
    //                "/App_Plugins/UmbCheckout/js/umbcheckout.metadata.propertyeditor.controller.js",
    //                "/App_Plugins/UmbCheckout/js/umbcheckout.resources.js",
    //                "/App_Plugins/UmbCheckout/js/umbcheckout.controller.js"
    //            },
    //            Stylesheets = new []
    //            {
    //                "/App_Plugins/UmbCheckout/css/umbcheckout.css"
    //            }
    //        });
    //    }
    //}

    internal sealed class UmbCheckoutManifestFilter : IPackageManifestReader
    {
        public Task<IEnumerable<PackageManifest>> ReadPackageManifestsAsync()
        {
            List<PackageManifest> manifest = [
                new()
                {
                    Id = Consts.PackageName,
                    Name = Consts.PackageName,
                    AllowTelemetry = true,
                    Version = UmbCheckoutVersion.Version.ToString(3),
                    Extensions = []
                }
            ];

            return Task.FromResult(manifest.AsEnumerable());
        }
    }
}
