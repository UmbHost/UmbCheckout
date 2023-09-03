using UmbCheckout.Backoffice.Files;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace UmbCheckout.Backoffice.Composers
{
    /// <summary>
    /// Registers static files used by the backoffice
    /// </summary>
    public class RegisterFilesComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.BackOfficeAssets()
                .Append<UmbCheckoutSettingsJsFIle>();
            builder.BackOfficeAssets()
                .Append<UmbCheckoutMetaDataPropertyEditorJsFIle>();
            builder.BackOfficeAssets()
                .Append<UmbCheckoutResourcesJsFIle>();
            builder.BackOfficeAssets()
                .Append<UmbCheckoutCssFIle>();
        }
    }
}
