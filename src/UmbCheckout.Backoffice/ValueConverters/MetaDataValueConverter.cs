using System.Text.Json;
using UmbCheckout.Backoffice.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace UmbCheckout.Backoffice.ValueConverters;

/// <summary>
/// The MetaData property editor value converter
/// </summary>
public class MetaDataValueConverter : PropertyValueConverterBase
{
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorUiAlias.InvariantEquals("UmbCheckout.PropertyEditorUi.MetaData");

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(IEnumerable<string>);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public override object ConvertSourceToIntermediate(IPublishedElement? owner, IPublishedPropertyType? propertyType, object? source, bool preview)
    {
        var sourceString = source?.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(sourceString))
        {
            return new Dictionary<string, string>();
        }

        var metaDataDictionary  = new Dictionary<string, string>();
        var values = JsonSerializer.Deserialize<IEnumerable<UmbCheckoutMetaData>>(sourceString);
        if (values != null)
        {
            foreach (var metaDataItem in values)
            {
                metaDataDictionary.Add(metaDataItem.Name, metaDataItem.Value);
            }
        }
        return metaDataDictionary;
    }
}
