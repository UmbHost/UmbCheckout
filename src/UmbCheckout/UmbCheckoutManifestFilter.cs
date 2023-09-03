using UmbCheckout.Shared;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;

namespace UmbCheckout
{
    public class UmbCheckoutManifest : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.ManifestFilters().Append<UmbCheckoutManifestFilter>();
        }
    }

    public class UmbCheckoutManifestFilter : IManifestFilter
    {
        public void Filter(List<PackageManifest> manifests)
        {
            manifests.Add(new PackageManifest
            {
                PackageName = Consts.PackageName,
                Version = UmbCheckoutVersion.Version.ToString(3),
                AllowPackageTelemetry = true,
                BundleOptions = BundleOptions.None
            });
        }
    }
}
