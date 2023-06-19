using Microsoft.Extensions.Logging;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Pocos;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Infrastructure.Scoping;
using ShippingRate = UmbCheckout.Stripe.Models.ShippingRate;

namespace UmbCheckout.Stripe.Services
{
    internal class StripeShippingRateDatabaseService : IStripeShippingRateDatabaseService
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IUmbracoMapper _mapper;
        private readonly ILogger<StripeShippingRateDatabaseService> _logger;

        public StripeShippingRateDatabaseService(IScopeProvider scopeProvider, IUmbracoMapper mapper, ILogger<StripeShippingRateDatabaseService> logger)
        {
            _scopeProvider = scopeProvider;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ShippingRate>> GetShippingRates()
        {
            try
            { 
                using var scope = _scopeProvider.CreateScope(autoComplete: true);
                var results = await scope.Database.FetchAsync<UmbCheckoutStripeShipping>();

                return _mapper.MapEnumerable<UmbCheckoutStripeShipping, ShippingRate>(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public async Task<ShippingRate?> GetShippingRate(long id)
        {
            try
            {
                using var scope = _scopeProvider.CreateScope(autoComplete: true);
                var results = await scope.Database.QueryAsync<UmbCheckoutStripeShipping>().SingleOrDefault(x => x.Id == id);

                return _mapper.Map<UmbCheckoutStripeShipping, ShippingRate>(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public async Task<ShippingRate?> UpdateShippingRate(ShippingRate shippingRate)
        {
            try
            {
                using var scope = _scopeProvider.CreateScope(autoComplete: true);

                var shippingRatePoco = _mapper.Map<ShippingRate, UmbCheckoutStripeShipping>(shippingRate);

                var existingShippingRate = await GetShippingRate(shippingRate.Id);

                long result;
                if (existingShippingRate == null)
                {
                    result = (long)await scope.Database.InsertAsync(shippingRatePoco);
                }
                else
                {
                    result = await scope.Database.UpdateAsync(shippingRatePoco);
                }

                return await GetShippingRate(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteShippingRate(long id)
        {
            try
            {
                using var scope = _scopeProvider.CreateScope(autoComplete: true);

                var existingShippingRate = await GetShippingRate(id);

                if (existingShippingRate == null)
                {
                    return false;
                }

                _ = await scope.Database.DeleteAsync(existingShippingRate);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
