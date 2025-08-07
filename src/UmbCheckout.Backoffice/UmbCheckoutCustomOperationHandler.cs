using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Umbraco.Cms.Api.Common.OpenApi;

namespace UmbCheckout.Backoffice;

public class UmbCheckoutCustomOperationHandler : IOperationIdHandler
{
    public bool CanHandle(ApiDescription apiDescription)
    {
        if (apiDescription.ActionDescriptor is not
            ControllerActionDescriptor controllerActionDescriptor)
            return false;

        return CanHandle(apiDescription, controllerActionDescriptor);
    }

    public bool CanHandle(ApiDescription apiDescription, ControllerActionDescriptor controllerActionDescriptor)
        => controllerActionDescriptor.ControllerTypeInfo.Namespace?.StartsWith("UmbCheckout") is true;

    public string Handle(ApiDescription apiDescription)
        => $"{apiDescription.ActionDescriptor.RouteValues["action"]}";
}