using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Scoping;

namespace UmbCheckout.Testing;

public static class TestMapper
{
    /// <summary>Every project's definitions class is named UmbCheckoutConfigurationMappingDefinition; pass the ones the test needs, namespace-qualified.</summary>
    public static IUmbracoMapper Create(params IMapDefinition[] definitions) =>
        new UmbracoMapper(new MapDefinitionCollection(() => definitions), Mock.Of<ICoreScopeProvider>(), NullLogger<UmbracoMapper>.Instance);
}
