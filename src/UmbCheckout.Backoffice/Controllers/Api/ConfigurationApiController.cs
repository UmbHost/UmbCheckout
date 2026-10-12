using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UmbCheckout.Backoffice.Mapping;
using UmbCheckout.Backoffice.Models.Api;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.ManagementApi;
using UmbCheckout.Shared.Models;
using UmbHost.Licensing.Models;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;

namespace UmbCheckout.Backoffice.Controllers.Api
{
    [ApiVersion("1.0")]
    [VersionedApiBackOfficeRoute("umbcheckout/configuration")]
    [ApiExplorerSettings(GroupName = "Configuration")]
    [MapToApi(UmbCheckoutApiComposer.DocumentName)]
    public class ConfigurationApiController : ManagementApiControllerBase
    {
        private readonly IConfigurationService _configurationService;
        private readonly IPublishedContentCache _contentCache;
        private readonly IPublishedUrlProvider _urlProvider;
        private readonly WebRoutingSettings _webRoutingSettings;
        private readonly Func<bool> _isLicensed;

        [ActivatorUtilitiesConstructor]
        public ConfigurationApiController(IConfigurationService configurationService, IPublishedContentCache contentCache, IPublishedUrlProvider urlProvider, IOptions<WebRoutingSettings> webRoutingSettings, UmbHost.Licensing.Services.LicenseService licenseService)
            : this(configurationService, contentCache, urlProvider, webRoutingSettings, () => UmbCheckoutSettings.IsLicensed)
        {
            licenseService.RunLicenseCheck();
        }

        internal ConfigurationApiController(IConfigurationService configurationService, IPublishedContentCache contentCache, IPublishedUrlProvider urlProvider, IOptions<WebRoutingSettings> webRoutingSettings, Func<bool> isLicensed)
        {
            _configurationService = configurationService;
            _contentCache = contentCache;
            _urlProvider = urlProvider;
            _webRoutingSettings = webRoutingSettings.Value;
            _isLicensed = isLicensed;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ConfigurationResponseModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetConfiguration()
        {
            var configuration = await _configurationService.GetConfiguration() ?? new UmbCheckoutConfiguration();
            return Ok(ConfigurationModelMapper.ToResponse(configuration, _isLicensed()));
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateConfiguration([FromBody] UpdateConfigurationRequestModel model)
        {
            if (!TryResolvePage(model.SuccessPageKey, out var successPage) || !TryResolvePage(model.CancelPageKey, out var cancelPage))
            {
                return ConfigurationOperationStatusResult(ConfigurationOperationStatus.PageNotFound);
            }

            var configuration = ConfigurationModelMapper.ToConfiguration(model, _isLicensed(), successPage, cancelPage);
            var updated = await _configurationService.UpdateConfiguration(configuration);

            return updated ? Ok() : ConfigurationOperationStatusResult(ConfigurationOperationStatus.SaveFailed);
        }

        [HttpGet("application-url")]
        [ProducesResponseType(typeof(ApplicationUrlStatusResponseModel), StatusCodes.Status200OK)]
        public IActionResult GetApplicationUrlStatus()
            => Ok(new ApplicationUrlStatusResponseModel { IsSet = !string.IsNullOrWhiteSpace(_webRoutingSettings.UmbracoApplicationUrl) });

        private bool TryResolvePage(Guid? key, out IEnumerable<MultiUrlPicker> picker)
        {
            picker = Enumerable.Empty<MultiUrlPicker>();
            if (key is null)
            {
                return true;
            }

            IPublishedContent? page = _contentCache.GetById(key.Value);
            if (page is null)
            {
                return false;
            }

            // The stored URL is the checkout fallback used by StripeSessionService when the page URL cannot be resolved at runtime.
            picker = [ConfigurationModelMapper.ToPicker(page.Key, page.Name, _urlProvider.GetUrl(page, UrlMode.Default, null, null), true)];
            return true;
        }

        private IActionResult ConfigurationOperationStatusResult(ConfigurationOperationStatus status) =>
            OperationStatusResult(status, problemDetails => status switch
            {
                ConfigurationOperationStatus.PageNotFound => BadRequest(problemDetails
                    .WithTitle(UmbCheckoutProblemDetails.InvalidValueTitle)
                    .WithDetail("The selected success or cancel page could not be found.")
                    .Build()),
                _ => BadRequest(problemDetails
                    .WithTitle("Configuration not saved")
                    .WithDetail("The configuration could not be saved.")
                    .Build())
            });
    }
}
