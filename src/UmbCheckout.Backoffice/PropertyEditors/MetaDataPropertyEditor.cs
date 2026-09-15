using UmbCheckout.Shared;
using Umbraco.Cms.Core.PropertyEditors;

namespace UmbCheckout.Backoffice.PropertyEditors
{
    /// <summary>
    /// The MetaData property editor
    /// </summary>
    /// <remarks>
    /// The backoffice UI for this editor is registered as the UmbCheckout.PropertyEditorUi.MetaData extension
    /// </remarks>
    [DataEditor(Consts.PropertyEditors.MetaDataAlias)]
    public class MetaDataPropertyEditor : DataEditor
    {
        public MetaDataPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
            : base(dataValueEditorFactory)
        {
        }
    }
}
