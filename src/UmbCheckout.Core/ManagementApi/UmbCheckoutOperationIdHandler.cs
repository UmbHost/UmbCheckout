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

        public UmbCheckoutOperationIdHandler(IOptions<ApiVersioningOptions> apiVersioningOptions, string controllerNamespacePrefix)
            : base(apiVersioningOptions)
            => _controllerNamespacePrefix = controllerNamespacePrefix;

        protected override bool CanHandle(ApiDescription apiDescription, ControllerActionDescriptor controllerActionDescriptor)
            => controllerActionDescriptor.ControllerTypeInfo.Namespace?.StartsWith(_controllerNamespacePrefix, StringComparison.Ordinal) is true;

        public override string Handle(ApiDescription apiDescription)
            => $"{apiDescription.ActionDescriptor.RouteValues["action"]}";
    }
}
