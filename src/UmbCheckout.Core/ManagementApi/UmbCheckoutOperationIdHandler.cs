using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.OpenApi;

namespace UmbCheckout.Core.ManagementApi
{
    /// <summary>
    /// Uses the action name as the operation ID for controllers in the given namespace, so the generated TypeScript SDK has readable method names.
    /// </summary>
    public class UmbCheckoutOperationIdHandler : OperationIdHandler
    {
        private readonly string _controllerNamespacePrefix;
        private readonly string? _excludedNamespacePrefix;

        public UmbCheckoutOperationIdHandler(IOptions<ApiVersioningOptions> apiVersioningOptions, string controllerNamespacePrefix, string? excludedNamespacePrefix = null)
            : base(apiVersioningOptions)
        {
            _controllerNamespacePrefix = controllerNamespacePrefix;
            _excludedNamespacePrefix = excludedNamespacePrefix;
        }

        protected override bool CanHandle(ApiDescription apiDescription, ControllerActionDescriptor controllerActionDescriptor)
            => UmbCheckoutNamespaceScope.Matches(controllerActionDescriptor.ControllerTypeInfo.Namespace, _controllerNamespacePrefix, _excludedNamespacePrefix);

        public override string Handle(ApiDescription apiDescription)
            => $"{apiDescription.ActionDescriptor.RouteValues["action"]}";
    }
}
