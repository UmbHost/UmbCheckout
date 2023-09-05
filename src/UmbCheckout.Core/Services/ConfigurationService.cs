using Microsoft.Extensions.Logging;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Notifications.Configuration;
using UmbHost.Licensing.Models;
using UmbHost.Licensing.Services;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Scoping;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;
using UmbCheckoutConfiguration = UmbCheckout.Core.Pocos.UmbCheckoutConfiguration;

namespace UmbCheckout.Core.Services
{
    /// <summary>
    /// A service which Gets or Updates the configuration
    /// </summary>
    internal class ConfigurationService : IConfigurationService
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly ICoreScopeProvider _coreScopeProvider;
        private readonly IUmbracoMapper _mapper;
        private readonly ILogger<ConfigurationService> _logger;

        public ConfigurationService(IScopeProvider scopeProvider, ILogger<ConfigurationService> logger, IUmbracoMapper mapper, ICoreScopeProvider coreScopeProvider, LicenseService licenseService)
        {
            _scopeProvider = scopeProvider;
            _logger = logger;
            _mapper = mapper;
            _coreScopeProvider = coreScopeProvider;
            licenseService.RunLicenseCheck();
        }

        /// <inheritdoc />
        public async Task<Shared.Models.UmbCheckoutConfiguration?> GetConfiguration()
        {
            try
            {
                using var scope = _scopeProvider.CreateScope(autoComplete: true);
                var result = await scope.Database.QueryAsync<UmbCheckoutConfiguration>().SingleOrDefault();

                if (!UmbCheckoutSettings.IsLicensed)
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
                if (!UmbCheckoutSettings.IsLicensed)
                {
                    configuration.StoreBasketInDatabase = false;
                    configuration.StoreBasketInCookie = false;
                }

                using var coreScope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                var configurationPoco = _mapper.Map<Shared.Models.UmbCheckoutConfiguration, UmbCheckoutConfiguration>(configuration);
                var existingConfiguration = await GetConfiguration();
                if (existingConfiguration != null)
                {
                    if (configurationPoco != null)
                    {
                        configurationPoco.Id = existingConfiguration.Id;
                        configurationPoco.Key = existingConfiguration.Key;
                        using var scope = _scopeProvider.CreateScope(autoComplete: true);
                        var db = scope.Database;
                        var result = await db.UpdateAsync(configurationPoco);
                        var updatedConfiguration = await GetConfiguration();
                        scope.Notifications.Publish(new OnConfigurationSavedNotification(updatedConfiguration));
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
                if (!UmbCheckoutSettings.IsLicensed)
                {
                    configuration.StoreBasketInDatabase = false;
                    configuration.StoreBasketInCookie = false;
                }

                using var scope = _scopeProvider.CreateScope(autoComplete: true);
                var db = scope.Database;
                var result = await db.InsertAsync(configuration);

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
