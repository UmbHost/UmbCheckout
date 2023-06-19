using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Models;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

namespace UmbCheckout.Stripe.Controllers.BackOffice.Api
{
    [PluginController("UmbCheckout")]
    public class ShippingRatesApiController : UmbracoAuthorizedApiController
    {
        private readonly ILogger<ShippingRatesApiController> _logger;
        private readonly IStripeShippingRateDatabaseService _stripeDatabaseService;

        public ShippingRatesApiController(ILogger<ShippingRatesApiController> logger, IStripeShippingRateDatabaseService stripeDatabaseService)
        {
            _logger = logger;
            _stripeDatabaseService = stripeDatabaseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetShippingRates()
        {
            try
            {
                var shippingRates = await _stripeDatabaseService.GetShippingRates();

                return new JsonResult(shippingRates, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetShippingRate(long? id)
        {
            try
            { 
                var shippingRateProperties = await GetShippingRateProperties(id);

                return new JsonResult(shippingRateProperties, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        [HttpPatch]
        public async Task<IActionResult> UpdateShippingRate([FromBody] ShippingRate shippingRate)
        {
            if (ModelState.IsValid)
            {
                var updatedShippingRate = await _stripeDatabaseService.UpdateShippingRate(shippingRate);
                var shippingRateProperties = await GetShippingRateProperties(updatedShippingRate?.Id);
                return new JsonResult(shippingRateProperties, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }

            return BadRequest();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteShippingRate([FromQuery] long id)
        {
            if (ModelState.IsValid)
            {
                var deletedShippingRate = await _stripeDatabaseService.DeleteShippingRate(id);

                if (deletedShippingRate == false)
                {
                    return BadRequest();
                }

                return Accepted();
            }

            return BadRequest();
        }

        private async Task<List<Property>> GetShippingRateProperties(long? id)
        {
            try
            {
                var shippingRate = new ShippingRate();
                if (id.HasValue)
                {
                    shippingRate = await _stripeDatabaseService.GetShippingRate(id.Value);
                }
                var backOfficeProperties = new List<Property>
                {
                    new()
                    {
                        Alias = "name",
                        Description = "The Shipping Rate Name",
                        Label = "Shipping Rate Name",
                        Value = shippingRate != null ? shippingRate.Name : string.Empty,
                        View = "textbox",
                        Validation = new Validation
                        {
                            Mandatory = true
                        }
                    },
                    new()
                    {
                        Alias = "value",
                        Description = "The Shipping Rate ID set in Stripe",
                        Label = "Shipping Rate ID",
                        Value = shippingRate != null ? shippingRate.Value : string.Empty,
                        View = "textbox",
                        Validation = new Validation
                        {
                            Mandatory = true
                        }
                    }
                };

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
