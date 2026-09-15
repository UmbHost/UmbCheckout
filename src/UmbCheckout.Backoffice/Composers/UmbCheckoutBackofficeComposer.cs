using Microsoft.Extensions.DependencyInjection;
using UmbCheckout.Backoffice.Manifests;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Manifest;

namespace UmbCheckout.Backoffice.Composers
{
    /// <summary>
    /// Registers the backoffice package manifest
    /// </summary>
    public class UmbCheckoutBackofficeComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<IPackageManifestReader, UmbCheckoutPackageManifestReader>();
        }
    }
}
