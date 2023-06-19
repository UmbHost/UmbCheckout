using Microsoft.Extensions.Logging;
using System.ComponentModel;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Pocos;
using UmbHost.Licensing;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Infrastructure.Scoping;

namespace UmbCheckout.Core.Services
{
    /// <summary>
    /// A service which Gets or Updates the configuration
    /// </summary>
    [LicenseProvider(typeof(UmbLicensingProvider))]
    internal class ConfigurationService : IConfigurationService
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IUmbracoMapper _mapper;
        private readonly ILogger<ConfigurationService> _logger;
        private readonly bool _licenseIsValid;

        public ConfigurationService(IScopeProvider scopeProvider, ILogger<ConfigurationService> logger, IUmbracoMapper mapper)
        {
            _scopeProvider = scopeProvider;
            _logger = logger;
            _mapper = mapper;
            _licenseIsValid = LicenseManager.IsValid(typeof(ConfigurationService));
        }

        /// <inheritdoc />
        public async Task<Shared.Models.UmbCheckoutConfiguration?> GetConfiguration()
        {
            try
            {
                using var scope = _scopeProvider.CreateScope(autoComplete: true);
                var result = await scope.Database.QueryAsync<UmbCheckoutConfiguration>().SingleOrDefault();

                if (!_licenseIsValid)
                {
                    if (result != null)
                    {
                        result.StoreBasketInDatabase = false;
                        result.StoreBasketInDatabase = false;
                    }
                }

                return _mapper.Map<UmbCheckoutConfiguration, Shared.Models.UmbCheckoutConfiguration>(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> UpdateConfiguration(Shared.Models.UmbCheckoutConfiguration configuration)
        {
            try
            {
                if (!_licenseIsValid)
                {
                    configuration.StoreBasketInDatabase = false;
                    configuration.StoreBasketInCookie = false;
                }

                var configurationPoco = _mapper.Map<Shared.Models.UmbCheckoutConfiguration, UmbCheckoutConfiguration>(configuration);
                var existingConfiguration = await GetConfiguration();
                if (existingConfiguration != null)
                {
                    if (configurationPoco != null)
                    {
                        configurationPoco.Id = existingConfiguration.Id;
                        using var scope = _scopeProvider.CreateScope(autoComplete: true);
                        var db = scope.Database;
                        var result = await db.UpdateAsync(configurationPoco);

                        return result != 0;
                    }
                }

                if (configurationPoco != null)
                {
                    var created = await CreateConfiguration(configurationPoco);

                    return created;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Inserts the initial configuration
        /// </summary>
        /// <param name="configuration">The configuration</param>
        /// <returns>true is inserted successfully false if not</returns>
        private async Task<bool> CreateConfiguration(UmbCheckoutConfiguration configuration)
        {
            try
            {
                if (!_licenseIsValid)
                {
                    configuration.StoreBasketInDatabase = false;
                    configuration.StoreBasketInCookie = false;
                }

                using var scope = _scopeProvider.CreateScope(autoComplete: true);
                var db = scope.Database;
                var result = (long?)await db.InsertAsync(configuration);

                return result is not null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
