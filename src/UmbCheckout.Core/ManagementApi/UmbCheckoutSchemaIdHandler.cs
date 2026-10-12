using Umbraco.Cms.Api.Common.OpenApi;

namespace UmbCheckout.Core.ManagementApi
{
    /// <summary>
    /// Uses the plain type name as the schema ID for models in the given namespace.
    /// </summary>
    public class UmbCheckoutSchemaIdHandler : SchemaIdHandler
    {
        private readonly string _modelNamespacePrefix;

        public UmbCheckoutSchemaIdHandler(string modelNamespacePrefix) => _modelNamespacePrefix = modelNamespacePrefix;

        public override bool CanHandle(Type type)
            => type.Namespace?.StartsWith(_modelNamespacePrefix, StringComparison.Ordinal) is true;

        public override string Handle(Type type) => type.IsGenericType ? base.Handle(type) : type.Name;
    }
}
