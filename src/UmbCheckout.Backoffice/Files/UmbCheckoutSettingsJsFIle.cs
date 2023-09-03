using Umbraco.Cms.Core.WebAssets;

namespace UmbCheckout.Backoffice.Files
{
    /// <summary>
    /// The configuration JavaScript file
    /// </summary>
    internal class UmbCheckoutSettingsJsFIle : JavaScriptFile
    {
        public UmbCheckoutSettingsJsFIle() : base("/App_Plugins/UmbCheckout/js/umbcheckout.controller.js")
        { }
    }
}
