using Umbraco.Cms.Core.Models.PublishedContent;

namespace UmbCheckout.Shared.Extensions
{
    public static class PublishedContentExtensions
    {
        public static bool HasTemplate(this IPublishedContent content)
        {
            return content.TemplateId is > 0;
        }
    }
}
