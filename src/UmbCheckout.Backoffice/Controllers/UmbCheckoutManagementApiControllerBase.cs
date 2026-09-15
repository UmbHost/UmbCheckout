using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UmbCheckout.Shared;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Routing;

namespace UmbCheckout.Backoffice.Controllers
{
    /// <summary>
    /// Base controller for the UmbCheckout backoffice Management API, available at /umbraco/umbcheckout/api/v1/
    /// </summary>
    [ApiController]
    [BackOfficeRoute(Consts.ApiName + "/api/v{version:apiVersion}")]
    [Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
    [MapToApi(Consts.ApiName)]
    public abstract class UmbCheckoutManagementApiControllerBase : ControllerBase
    {
    }
}
