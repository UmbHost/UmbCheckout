using Schema.NET;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace UmbCheckout.Core.Extensions
{
    public static class PublishedContentExtensions
    {
        public static HtmlEncodedString ToProductSchema(this IPublishedContent content, string currencyCode, string? nameAlias = null, string? descriptionAlias = null, string? imageAlias = null, string? skuAlias = null, string? mpnAlias = null, string? brandAlias = null)
        {
            var productSchema = new Product
            {
                Name = (!string.IsNullOrEmpty(nameAlias) ? content.Value<string>(nameAlias) : content.Name)!,
                Sku = (!string.IsNullOrEmpty(skuAlias) ? content.Value<string>(skuAlias) : content.Value<string>(Shared.Consts.PropertyAlias.Sku))!,
                Mpn = (!string.IsNullOrEmpty(mpnAlias) ? content.Value<string>(mpnAlias) : content.Value<string>(Shared.Consts.PropertyAlias.Mpn))!
            };

            if (!string.IsNullOrEmpty(imageAlias) && content.HasValue(imageAlias))
            {
                productSchema.Image = new Uri(content.Value<MediaWithCrops>(imageAlias)!.Url(mode: UrlMode.Absolute));
            }

            if (!string.IsNullOrEmpty(descriptionAlias))
            {
                productSchema.Description = content.Value<string>(descriptionAlias)!;
            }
            else if (content.HasValue(Shared.Consts.PropertyAlias.DescriptionAlias))
            {
                productSchema.Description = content.Value<string>(Shared.Consts.PropertyAlias.DescriptionAlias)!;
            }
            else if (content.HasValue(Shared.Consts.PropertyAlias.FallbackDescriptionAlias))
            {
                productSchema.Description = content.Value<string>(Shared.Consts.PropertyAlias.FallbackDescriptionAlias)!;
            }

            if (!string.IsNullOrEmpty(brandAlias))
            {
                productSchema.Brand = new Brand
                {
                    Name = content.Value<string>(brandAlias)!
                };
            }

            productSchema.Offers = new Offer
            {
                Url = new Uri(content.Url(mode: UrlMode.Absolute)),
                PriceCurrency = currencyCode,
                Price = content.Value<decimal>(Shared.Consts.PropertyAlias.PriceAlias)
            };

            return new HtmlEncodedString(productSchema.ToString());
        }
    }
}
