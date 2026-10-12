using Umbraco.Cms.Api.Common.OpenApi;

namespace UmbCheckout.Core.ManagementApi
{
    /// <summary>
    /// Uses the plain type name as the schema ID for models in the given namespace.
    /// </summary>
    public class UmbCheckoutSchemaIdHandler : SchemaIdHandler
    {
        private readonly string _modelNamespacePrefix;
        private readonly string? _excludedNamespacePrefix;

        public UmbCheckoutSchemaIdHandler(string modelNamespacePrefix, string? excludedNamespacePrefix = null)
        {
            _modelNamespacePrefix = modelNamespacePrefix;
            _excludedNamespacePrefix = excludedNamespacePrefix;
        }

        public override bool CanHandle(Type type)
            => UmbCheckoutNamespaceScope.Matches(type.Namespace, _modelNamespacePrefix, _excludedNamespacePrefix);

        public override string Handle(Type type) => type.IsGenericType ? base.Handle(type) : type.Name;
    }
}
