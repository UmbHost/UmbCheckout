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
            var productSchema = new Product();

            var name = !string.IsNullOrEmpty(nameAlias) ? content.Value<string>(nameAlias) : content.Name;
            if (name is not null)
            {
                productSchema.Name = name;
            }

            var sku = !string.IsNullOrEmpty(skuAlias) ? content.Value<string>(skuAlias) : content.Value<string>(Shared.Consts.PropertyAlias.Sku);
            if (sku is not null)
            {
                productSchema.Sku = sku;
            }

            var mpn = !string.IsNullOrEmpty(mpnAlias) ? content.Value<string>(mpnAlias) : content.Value<string>(Shared.Consts.PropertyAlias.Mpn);
            if (mpn is not null)
            {
                productSchema.Mpn = mpn;
            }

            if (!string.IsNullOrEmpty(imageAlias) && content.HasValue(imageAlias))
            {
                var image = content.Value<MediaWithCrops>(imageAlias);
                if (image is not null)
                {
                    productSchema.Image = new Uri(image.Url(mode: UrlMode.Absolute));
                }
            }

            var description = !string.IsNullOrEmpty(descriptionAlias)
                ? content.Value<string>(descriptionAlias)
                : content.HasValue(Shared.Consts.PropertyAlias.DescriptionAlias)
                    ? content.Value<string>(Shared.Consts.PropertyAlias.DescriptionAlias)
                    : content.HasValue(Shared.Consts.PropertyAlias.FallbackDescriptionAlias)
                        ? content.Value<string>(Shared.Consts.PropertyAlias.FallbackDescriptionAlias)
                        : null;
            if (description is not null)
            {
                productSchema.Description = description;
            }

            if (!string.IsNullOrEmpty(brandAlias))
            {
                var brandName = content.Value<string>(brandAlias);
                if (brandName is not null)
                {
                    productSchema.Brand = new Brand
                    {
                        Name = brandName
                    };
                }
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
