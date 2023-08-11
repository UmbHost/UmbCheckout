using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UmbCheckout.Backoffice.Models;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared;
using UmbCheckout.Shared.Extensions;
using UmbHost.Licensing.Models;
using UmbHost.Licensing.Services;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

namespace UmbCheckout.Backoffice.Controllers.Api
{
    /// <summary>
    /// UmbracoAuthorizedApiController to retrieve the configuration settings for the backoffice
    /// </summary>
    [PluginController(Consts.PackageName)]
    public class ConfigurationApiController : UmbracoAuthorizedApiController
    {
        private readonly IConfigurationService _configuration;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly ILogger<ConfigurationApiController> _logger;

        public ConfigurationApiController(IConfigurationService configuration, ILogger<ConfigurationApiController> logger, LicenseService licenseService, ILocalizedTextService localizedTextService)
        {
            _configuration = configuration;
            _logger = logger;
            _localizedTextService = localizedTextService;
            licenseService.RunLicenseCheck();
        }

        /// <summary>
        /// Gets the configuration properties
        /// </summary>
        /// <returns>The configuration properties in JSON</returns>
        [HttpGet]
        public async Task<IActionResult> GetConfiguration()
        {
            try
            {
                var backOfficeProperties = await GetConfigurationProperties();

                return new JsonResult(backOfficeProperties, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets the license status
        /// </summary>
        /// <returns>The license status as a string</returns>
        [HttpGet]
        public IActionResult GetLicenseStatus()
        {
            var model = new LicenseStatus
            {
                Status = UmbCheckoutSettings.LicenseStatus,
                RegDate = UmbCheckoutSettings.LicenseDetails.RegDate,
                ExpiryDateTime = UmbCheckoutSettings.LicenseDetails.ExpiryDateTime,
                ValidDomains = UmbCheckoutSettings.LicenseDetails.ValidDomains,
                ValidPaths = UmbCheckoutSettings.LicenseDetails.ValidPaths,
                LicenseAddons = UmbCheckoutSettings.LicenseDetails.LicenseAddons
            };
            return new JsonResult(model);
        }

        /// <summary>
        /// Updates the configuration
        /// </summary>
        /// <param name="configValues">The configuration values</param>
        /// <returns>The updated configuration properties in JSON</returns>
        [HttpPatch]
        public async Task<IActionResult> UpdateConfiguration([FromBody] ConfigurationValue configValues)
        {
            try
            {
                var storeBasketInCookie =
                    configValues.StoreBasketInCookie.ToBoolean();
                var storeBasketInDatabase =
                    configValues.StoreBasketInDatabase.ToBoolean();
                var enableShipping =
                    configValues.EnableShipping.ToBoolean();

                if (!UmbCheckoutSettings.IsLicensed)
                {
                    storeBasketInCookie = false;
                    storeBasketInDatabase = false;
                }
                else
                {
                    if (storeBasketInDatabase)
                    {
                        storeBasketInCookie = true;
                    }
                }

                var configuration = new Shared.Models.UmbCheckoutConfiguration
                {
                    BasketInCookieExpiry = configValues.BasketInCookieExpiry,
                    BasketInDatabaseExpiry = configValues.BasketInDatabaseExpiry,
                    StoreBasketInCookie = storeBasketInCookie,
                    StoreBasketInDatabase = storeBasketInDatabase,
                    SuccessPageUrl = configValues.SuccessPageUrl,
                    CancelPageUrl = configValues.CancelPageUrl,
                    EnableShipping = enableShipping
                };

                var updated = await _configuration.UpdateConfiguration(configuration);

                if (updated)
                {
                    var backOfficeProperties = await GetConfigurationProperties();

                    return new JsonResult(backOfficeProperties, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Converts the configuration properties to a JSON string
        /// </summary>
        /// <returns>The configuration properties in JSON</returns>
        private async Task<List<Property>> GetConfigurationProperties()
        {
            try
            {
                var storeBasketInCookieDescription = _localizedTextService.Localize(Consts.LocalizationKeys.Area, Consts.LocalizationKeys.StoreBasketCookieDescription, CultureInfo.CurrentUICulture);
                var storeBasketInDatabaseDescription = _localizedTextService.Localize(Consts.LocalizationKeys.Area, Consts.LocalizationKeys.StoreBasketDatabaseDescription, CultureInfo.CurrentUICulture);
                if (!UmbCheckoutSettings.IsLicensed)
                {
                    storeBasketInCookieDescription += Environment.NewLine + _localizedTextService.Localize(Consts.LocalizationKeys.Area, Consts.LocalizationKeys.DisabledUnlicensed, CultureInfo.CurrentUICulture);
                    storeBasketInDatabaseDescription += Environment.NewLine + _localizedTextService.Localize(Consts.LocalizationKeys.Area, Consts.LocalizationKeys.DisabledUnlicensed, CultureInfo.CurrentUICulture);
                }

                var configurationDb = await _configuration.GetConfiguration();
                var backOfficeProperties = new List<Property>
                {
                    new()
                    {
                        Alias = "successPageUrl",
                        Description = _localizedTextService.Localize(Consts.LocalizationKeys.Area, Consts.LocalizationKeys.SuccessPageUrlDescription, CultureInfo.CurrentUICulture),
                        Label = _localizedTextService.Localize(Consts.LocalizationKeys.Area, Consts.LocalizationKeys.SuccessPageUrlLabel, CultureInfo.CurrentUICulture),
                        Value = configurationDb != null ? configurationDb.SuccessPageUrl : string.Empty,
                        View = "multiurlpicker",
                        Config = new Config
                        {
                            HideAnchor = false,
                            IgnoreUserStartNodes = false,
                            MaxNumber = 1,
                            MinNumber = 1,
                        },
                        Validation = new Validation
                        {
                            Mandatory = true
                        }
                    },
                    new()
                    {
                        Alias = "cancelPageUrl",
                        Description = _localizedTextService.Localize(Consts.LocalizationKeys.Area, Consts.LocalizationKeys.CancelPageUrlDescription, CultureInfo.CurrentUICulture),
                        Label = _localizedTextService.Localize(Consts.LocalizationKeys.Area, Consts.LocalizationKeys.CancelPageUrlLabel, CultureInfo.CurrentUICulture),
                        Value = configurationDb != null ? configurationDb.CancelPageUrl : string.Empty,
                        View = "multiurlpicker",
                        Config = new Config
                        {
                            HideAnchor = false,
                            IgnoreUserStartNodes = false,
                            MaxNumber = 1,
                            MinNumber = 1,
                        },
                        Validation = new Validation
                        {
                            Mandatory = true
                        }
                    },
                    new()
                    {
                        Alias = "enableShipping",
                        Description = _localizedTextService.Localize(Consts.LocalizationKeys.Area, Consts.LocalizationKeys.EnableShippingDescription, CultureInfo.CurrentUICulture),
                        Label = _localizedTextService.Localize(Consts.LocalizationKeys.Area, Consts.LocalizationKeys.EnableShippingLabel, CultureInfo.CurrentUICulture),
                        Value = configurationDb != null ? configurationDb.EnableShipping.ToString() : "false",
                        View = "boolean"
                    },
                };

                if (UmbCheckoutSettings.IsLicensed)
                {
                    backOfficeProperties.Add(new Property
                    {
                            Alias = "storeBasketInCookie",
                            Description = storeBasketInCookieDescription,
                            Label = "Store Basket In a Cookie",
                            Value = configurationDb != null ? configurationDb.StoreBasketInCookie.ToString() : "false",
                            View = "boolean"
                        });

                    backOfficeProperties.Add(new Property
                    {
                        Alias = "basketInCookieExpiry",
                        Description = _localizedTextService.Localize(Consts.LocalizationKeys.Area, Consts.LocalizationKeys.StoreBasketCookieExpiryDescription, CultureInfo.CurrentUICulture),
                        Label = _localizedTextService.Localize(Consts.LocalizationKeys.Area, Consts.LocalizationKeys.StoreBasketCookieExpiryLabel, CultureInfo.CurrentUICulture),
                        Value = configurationDb != null ? configurationDb.BasketInCookieExpiry.ToString() : "30",
                        View = "integer",
                        Validation = new Validation
                        {
                            Mandatory = true
                        }
                    });

                    backOfficeProperties.Add(new Property
                    {
                        Alias = "storeBasketInDatabase",
                        Description = storeBasketInDatabaseDescription,
                        Label = _localizedTextService.Localize(Consts.LocalizationKeys.Area, Consts.LocalizationKeys.StoreBasketCookieLabel, CultureInfo.CurrentUICulture),
                        Value = configurationDb != null ? configurationDb.StoreBasketInDatabase.ToString() : "false",
                        View = "boolean"
                    });

                    backOfficeProperties.Add(new Property
                    {
                        Alias = "basketInDatabaseExpiry",
                        Description = _localizedTextService.Localize(Consts.LocalizationKeys.Area, Consts.LocalizationKeys.StoreBasketDatabaseExpiryDescription, CultureInfo.CurrentUICulture),
                        Label = _localizedTextService.Localize(Consts.LocalizationKeys.Area, Consts.LocalizationKeys.StoreBasketDatabaseExpiryLabel, CultureInfo.CurrentUICulture),
                        Value = configurationDb != null ? configurationDb.BasketInDatabaseExpiry.ToString() : "30",
                        View = "integer",
                        Validation = new Validation
                        {
                            Mandatory = true
                        }
                    });
                }

                return backOfficeProperties;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
