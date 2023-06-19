using System.ComponentModel;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UmbCheckout.Backoffice.Models;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared;
using UmbCheckout.Shared.Extensions;
using UmbHost.Licensing;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

namespace UmbCheckout.Backoffice.Controllers.Api
{
    /// <summary>
    /// UmbracoAuthorizedApiController to retrieve the configuration settings for the backoffice
    /// </summary>
    [PluginController(Consts.PackageName)]
    [LicenseProvider(typeof(UmbLicensingProvider))]
    public class ConfigurationApiController : UmbracoAuthorizedApiController
    {
        private readonly IConfigurationService _configuration;
        private readonly ILogger<ConfigurationApiController> _logger;
        private readonly bool _licenseIsValid;

        public ConfigurationApiController(IConfigurationService configuration, ILogger<ConfigurationApiController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _licenseIsValid = LicenseManager.IsValid(typeof(ConfigurationApiController));
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
            if (_licenseIsValid)
            {
                return new JsonResult("Valid");
            }

            return new JsonResult("Invalid");
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

                if (!_licenseIsValid)
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
                var storeBasketInCookieDescription = "Whether the basket contents should be stored in a cookie";
                var storeBasketInDatabaseDescription = "Whether the basket contents should be stored in the database";
                if (!_licenseIsValid)
                {
                    storeBasketInCookieDescription += Environment.NewLine + "DISABLED IN UNLICENSED MODE";
                    storeBasketInDatabaseDescription += Environment.NewLine + "DISABLED IN UNLICENSED MODE";
                }

                var configurationDb = await _configuration.GetConfiguration();
                var backOfficeProperties = new List<Property>
                {
                    new()
                    {
                        Alias = "successPageUrl",
                        Description = "The checkout success page",
                        Label = "Success Page URL",
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
                        Description = "The checkout cancelled page",
                        Label = "Cancel Page URL",
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
                        Description = "Enable the use of Shipping Rates",
                        Label = "Enable Shipping",
                        Value = configurationDb != null ? configurationDb.EnableShipping.ToString() : "false",
                        View = "boolean"
                    },
                };

                if (_licenseIsValid)
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
                        Description = "How many days the basket cookie should persist",
                        Label = "Basket Expiry (Cookie)",
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
                        Label = "Store Basket In The Database",
                        Value = configurationDb != null ? configurationDb.StoreBasketInDatabase.ToString() : "false",
                        View = "boolean"
                    });

                    backOfficeProperties.Add(new Property
                    {
                        Alias = "basketInDatabaseExpiry",
                        Description = "How many days the basket should persist in the database",
                        Label = "Basket Expiry (Database)",
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
