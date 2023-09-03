using Umbraco.Cms.Core.PropertyEditors;

namespace UmbCheckout.Backoffice.PropertyEditors
{
    /// <summary>
    /// The MetaData property editor
    /// </summary>
    [DataEditor(
        alias: "umbCheckoutMetaData",
        name: "Meta Data",
        view: "~/App_Plugins/UmbCheckout/propertyeditors/metaData.html",
        Group = "eCommerce",
        Icon = "icon-ordered-list")]
    public class MetaDataPropertyEditor : DataEditor
    {
        public MetaDataPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
            : base(dataValueEditorFactory)
        {
        }
    }
}
