using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UmbCheckout.Backoffice.Models;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;

namespace UmbCheckout.Backoffice.Controllers
{
    /// <summary>
    /// Retrieves and updates the UmbCheckout configuration for the backoffice
    /// </summary>
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "Configuration")]
    public class ConfigurationController : UmbCheckoutManagementApiControllerBase
    {
        private readonly IConfigurationService _configuration;
        private readonly ILogger<ConfigurationController> _logger;
        private readonly WebRoutingSettings _umbracoWebRoutingSettings;

        public ConfigurationController(IConfigurationService configuration, ILogger<ConfigurationController> logger, IOptions<WebRoutingSettings> umbracoWebRoutingSettings)
        {
            _configuration = configuration;
            _logger = logger;
            _umbracoWebRoutingSettings = umbracoWebRoutingSettings.Value;
            // TODO: UmbHost.Licensing - re-enable once a Umbraco 17 compatible version of UmbHost.Licensing is available
            //licenseService.RunLicenseCheck();
        }

        /// <summary>
        /// Gets the configuration
        /// </summary>
        [HttpGet("configuration")]
        [ProducesResponseType<ConfigurationResponseModel>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetConfiguration()
        {
            try
            {
                return Ok(await GetConfigurationResponse());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Updates the configuration
        /// </summary>
        /// <param name="model">The configuration values</param>
        /// <returns>The updated configuration</returns>
        [HttpPut("configuration")]
        [ProducesResponseType<ConfigurationResponseModel>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateConfiguration([FromBody] ConfigurationModel model)
        {
            try
            {
                var storeBasketInCookie = model.StoreBasketInCookie;
                var storeBasketInDatabase = model.StoreBasketInDatabase;

                // TODO: UmbHost.Licensing - re-enable once a Umbraco 17 compatible version of UmbHost.Licensing is available
                //if (!UmbCheckoutSettings.IsLicensed)
                //{
                    storeBasketInCookie = false;
                    storeBasketInDatabase = false;
                //}
                //else
                //{
                //    if (storeBasketInDatabase)
                //    {
                //        storeBasketInCookie = true;
                //    }
                //}

                var configuration = new UmbCheckoutConfiguration
                {
                    BasketInCookieExpiry = model.BasketInCookieExpiry,
                    BasketInDatabaseExpiry = model.BasketInDatabaseExpiry,
                    StoreBasketInCookie = storeBasketInCookie,
                    StoreBasketInDatabase = storeBasketInDatabase,
                    SuccessPageUrl = model.SuccessPageUrl.Select(x => x.ToMultiUrlPicker()).ToList(),
                    CancelPageUrl = model.CancelPageUrl.Select(x => x.ToMultiUrlPicker()).ToList(),
                    EnableShipping = model.EnableShipping,
                    CurrencyCode = model.CurrencyCode.ToUpperInvariant()
                };

                var updated = await _configuration.UpdateConfiguration(configuration);

                if (updated)
                {
                    return Ok(await GetConfigurationResponse());
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        private async Task<ConfigurationResponseModel> GetConfigurationResponse()
        {
            var configuration = await _configuration.GetConfiguration();

            return new ConfigurationResponseModel
            {
                SuccessPageUrl = ToLinkPickerValue(configuration?.SuccessPageUrl),
                CancelPageUrl = ToLinkPickerValue(configuration?.CancelPageUrl),
                CurrencyCode = configuration?.CurrencyCode ?? string.Empty,
                EnableShipping = configuration?.EnableShipping ?? false,
                StoreBasketInCookie = configuration?.StoreBasketInCookie ?? false,
                BasketInCookieExpiry = configuration?.BasketInCookieExpiry ?? 30,
                StoreBasketInDatabase = configuration?.StoreBasketInDatabase ?? false,
                BasketInDatabaseExpiry = configuration?.BasketInDatabaseExpiry ?? 30,
                // TODO: UmbHost.Licensing - re-enable once a Umbraco 17 compatible version of UmbHost.Licensing is available
                //IsLicensed = UmbCheckoutSettings.IsLicensed,
                IsLicensed = false,
                HasUmbracoApplicationUrlSet = !string.IsNullOrWhiteSpace(_umbracoWebRoutingSettings.UmbracoApplicationUrl)
            };
        }

        /// <summary>
        /// Converts values stored by earlier versions (using udi) to the link picker format used by the Umbraco 14+ backoffice
        /// </summary>
        private static IEnumerable<MultiUrlPicker> ToLinkPickerValue(IEnumerable<MultiUrlPicker>? links)
        {
            if (links == null)
            {
                return [];
            }

            return links.Select(link =>
            {
                if (string.IsNullOrEmpty(link.Unique))
                {
                    var documentKey = link.GetDocumentKey();
                    if (documentKey.HasValue)
                    {
                        link.Unique = documentKey.Value.ToString();
                        link.Type = Constants.UdiEntityType.Document;
                    }
                    else if (!string.IsNullOrEmpty(link.Url))
                    {
                        link.Type = "external";
                    }
                }

                return link;
            }).ToList();
        }
    }
}
