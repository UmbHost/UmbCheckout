using UmbCheckout.Stripe.Models;
using UmbCheckout.Stripe.Pocos;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace UmbCheckout.Stripe.Composers
{
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
            mapper.Define<ShippingRate, UmbCheckoutStripeShipping>((_, _) => new UmbCheckoutStripeShipping(), Map);
            mapper.Define<UmbCheckoutStripeShipping, ShippingRate>((_, _) => new ShippingRate(), Map);
        }

        private static void Map(ShippingRate source, UmbCheckoutStripeShipping target, MapperContext context)
        {
            target.Id = source.Id;
            target.Name = source.Name;
            target.Value = source.Value;
        }

        private static void Map(UmbCheckoutStripeShipping source, ShippingRate target, MapperContext context)
        {
            target.Id = source.Id;
            target.Name = source.Name;
            target.Value = source.Value;
        }
    }
}
