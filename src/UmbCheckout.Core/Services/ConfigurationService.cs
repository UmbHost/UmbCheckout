using Microsoft.Extensions.Logging;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Notifications.Configuration;
using UmbCheckout.Shared.Persistence;
using UmbHost.Licensing.Services;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Scoping;
using IScopeAccessor = Umbraco.Cms.Infrastructure.Scoping.IScopeAccessor;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;
using UmbCheckoutConfiguration = UmbCheckout.Core.Pocos.UmbCheckoutConfiguration;

namespace UmbCheckout.Core.Services
{
    /// <summary>
    /// A service which Gets or Updates the configuration. The basket storage options are stored and read as given: basket storage is a
    /// licensed add-on feature, and the add-on applies its licence rules by wrapping this service.
    /// </summary>
    /// <remarks>
    /// The stored row is cached for the life of the process (a fresh model is mapped from it on every read). A save clears it here and,
    /// once committed, on every load-balanced server (<see cref="Cache.UmbCheckoutConfigurationSavedCacheHandler"/>).
    /// </remarks>
    internal class ConfigurationService : IConfigurationService
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly ICoreScopeProvider _coreScopeProvider;
        private readonly IUmbracoMapper _mapper;
        private readonly ILogger<ConfigurationService> _logger;
        private readonly SettingsCache<UmbCheckoutConfiguration> _cache;
        private readonly IScopeAccessor? _scopeAccessor;

        public ConfigurationService(IScopeProvider scopeProvider, ILogger<ConfigurationService> logger, IUmbracoMapper mapper, ICoreScopeProvider coreScopeProvider, LicenseService licenseService,
            SettingsCache<UmbCheckoutConfiguration> cache, IScopeAccessor? scopeAccessor = null)
        {
            _scopeProvider = scopeProvider;
            _logger = logger;
            _mapper = mapper;
            _coreScopeProvider = coreScopeProvider;
            _cache = cache;
            _scopeAccessor = scopeAccessor;
        }

        /// <inheritdoc />
        public async Task<Shared.Models.UmbCheckoutConfiguration?> GetConfiguration()
        {
            try
            {
                return _mapper.Map<UmbCheckoutConfiguration, Shared.Models.UmbCheckoutConfiguration>(await ReadAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>The stored row: cached, or read (and cached unless the read is inside someone else's scope).</summary>
        private async Task<UmbCheckoutConfiguration?> ReadAsync()
        {
            if (_cache.TryGet(out var cached))
            {
                return cached;
            }

            // Taken before the query: a save that clears the cache meanwhile stops this (possibly older) row being cached.
            var generation = _cache.Generation;
            // Inside someone else's scope (a uSync import, a save) the row may be uncommitted and could still roll back: read it, don't cache it.
            var insideAnotherScope = _scopeAccessor?.AmbientScope is not null;
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var result = await scope.Database.QueryAsync<UmbCheckoutConfiguration>().SingleOrDefault();
            if (!insideAnotherScope)
            {
                _cache.TrySet(result, generation);
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateConfiguration(Shared.Models.UmbCheckoutConfiguration configuration)
        {
            try
            {
                using (_coreScopeProvider.CreateCoreScope(autoComplete: true))
                {
                    return await SaveAsync(configuration);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
            finally
            {
                // This server's copy, after the commit. Other servers, and a save inside an outer scope, are cleared by the Saved cache
                // handler, which Umbraco runs once the outermost scope has committed.
                _cache.Clear();
            }
        }

        private async Task<bool> SaveAsync(Shared.Models.UmbCheckoutConfiguration configuration)
        {
            var configurationPoco = _mapper.Map<Shared.Models.UmbCheckoutConfiguration, UmbCheckoutConfiguration>(configuration);
            if (configurationPoco == null)
            {
                return false;
            }

            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var db = scope.Database;
            // From the database, not the cache: another server's save may not have reached this one's cache yet.
            var existingConfiguration = await db.QueryAsync<UmbCheckoutConfiguration>().SingleOrDefault();
            bool saved;
            if (existingConfiguration != null)
            {
                configurationPoco.Id = existingConfiguration.Id;
                configurationPoco.Key = existingConfiguration.Key;
                saved = await db.UpdateAsync(configurationPoco) != 0;
            }
            else
            {
                // The first save publishes Saved too, so uSync writes its file and every server's cache is cleared.
                saved = await db.InsertAsync(configurationPoco) is not null;
            }

            if (saved)
            {
                // The row as now stored (the cache still holds the previous one until this save commits).
                var stored = await db.QueryAsync<UmbCheckoutConfiguration>().SingleOrDefault();
                scope.Notifications.Publish(new OnConfigurationSavedNotification(_mapper.Map<UmbCheckoutConfiguration, Shared.Models.UmbCheckoutConfiguration>(stored)));
            }

            return saved;
        }
    }
}
