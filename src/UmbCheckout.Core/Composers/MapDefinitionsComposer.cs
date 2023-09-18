using UmbCheckout.Core.Pocos;
using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using JsonSerializer = System.Text.Json.JsonSerializer;
using UmbCheckoutConfiguration = UmbCheckout.Core.Pocos.UmbCheckoutConfiguration;

namespace UmbCheckout.Core.Composers
{
    /// <summary>
    /// The poco to model mapper for the configuration values
    /// </summary>
    internal class MapDefinitionsComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
                .Add<UmbCheckoutConfigurationMappingDefinition>();
        }
    }

    public class UmbCheckoutConfigurationMappingDefinition : IMapDefinition
    {
        public void DefineMaps(IUmbracoMapper mapper)
        {
            mapper.Define<UmbCheckoutConfiguration, Shared.Models.UmbCheckoutConfiguration>((_, _) => new Shared.Models.UmbCheckoutConfiguration(), Map);
            mapper.Define<Shared.Models.UmbCheckoutConfiguration, UmbCheckoutConfiguration>((_, _) => new UmbCheckoutConfiguration(), Map);
            mapper.Define<UmbCheckoutBasket, Basket>((_, _) => new Basket(), Map);
            mapper.Define<Basket, UmbCheckoutBasket>((_, _) => new UmbCheckoutBasket(), Map);
        }

        private static void Map(UmbCheckoutConfiguration source, Shared.Models.UmbCheckoutConfiguration target, MapperContext context)
        {
            target.Id = source.Id;
            target.Key = source.Key;
            target.BasketInCookieExpiry = source.BasketInCookieExpiry;
            target.BasketInDatabaseExpiry = source.BasketInDatabaseExpiry;
            target.CancelPageUrl = (!string.IsNullOrEmpty(source.CancelPageUrl)
                ? JsonSerializer.Deserialize<IEnumerable<MultiUrlPicker>>(source.CancelPageUrl)
                : Enumerable.Empty<MultiUrlPicker>()) ?? Array.Empty<MultiUrlPicker>();

            target.SuccessPageUrl = (!string.IsNullOrEmpty(source.SuccessPageUrl) 
                ? JsonSerializer.Deserialize<IEnumerable<MultiUrlPicker>>(source.SuccessPageUrl) 
                : Enumerable.Empty<MultiUrlPicker>()) ?? Array.Empty<MultiUrlPicker>();
            target.StoreBasketInCookie = source.StoreBasketInCookie;
            target.StoreBasketInDatabase = source.StoreBasketInDatabase;
            target.EnableShipping = source.EnableShipping;
            target.CurrencyCode = source.CurrencyCode;
        }

        private static void Map(Shared.Models.UmbCheckoutConfiguration source, UmbCheckoutConfiguration target, MapperContext context)
        {
            target.Id = source.Id;
            target.Key = source.Key;
            target.BasketInCookieExpiry = source.BasketInCookieExpiry;
            target.BasketInDatabaseExpiry = source.BasketInDatabaseExpiry;
            target.CancelPageUrl = JsonSerializer.Serialize(source.CancelPageUrl);
            target.SuccessPageUrl = JsonSerializer.Serialize(source.SuccessPageUrl);
            target.StoreBasketInCookie = source.StoreBasketInCookie;
            target.StoreBasketInDatabase = source.StoreBasketInDatabase;
            target.EnableShipping = source.EnableShipping;
            target.CurrencyCode = source.CurrencyCode;
        }

        private static void Map(UmbCheckoutBasket source, Basket target, MapperContext context)
        {
            target.LineItems = JsonSerializer.Deserialize<IEnumerable<LineItem>>(source.LineItems) ?? Array.Empty<LineItem>();
            target.Total = source.BasketTotal;
        }

        private static void Map(Basket source, UmbCheckoutBasket target, MapperContext context)
        {
            target.LineItems = JsonSerializer.Serialize(source.LineItems);
            target.BasketTotal = source.Total;
        }
    }
}
