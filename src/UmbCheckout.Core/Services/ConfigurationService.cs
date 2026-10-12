using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Persistence;
using UmbCheckout.Shared.Notifications.Configuration;
using UmbHost.Licensing.Services;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using UmbCheckoutConfiguration = UmbCheckout.Core.Pocos.UmbCheckoutConfiguration;

namespace UmbCheckout.Core.Services
{
    /// <summary>
    /// A service which Gets or Updates the configuration
    /// </summary>
    internal class ConfigurationService : IConfigurationService
    {
        private readonly IEFCoreScopeProvider<UmbCheckoutDbContext> _scopeProvider;
        private readonly IUmbracoMapper _mapper;
        private readonly ILogger<ConfigurationService> _logger;

        public ConfigurationService(IEFCoreScopeProvider<UmbCheckoutDbContext> scopeProvider, ILogger<ConfigurationService> logger, IUmbracoMapper mapper, LicenseService licenseService)
        {
            _scopeProvider = scopeProvider;
            _logger = logger;
            _mapper = mapper;
            licenseService.RunLicenseCheck();
        }

        /// <inheritdoc />
        public async Task<Shared.Models.UmbCheckoutConfiguration?> GetConfigurationAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _scopeProvider.CreateScope();
            var entity = await scope.ExecuteWithContextAsync(db => db.Configurations.AsNoTracking().SingleOrDefaultAsync(cancellationToken));
            scope.Complete();

            return _mapper.Map<UmbCheckoutConfiguration, Shared.Models.UmbCheckoutConfiguration>(entity);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateConfigurationAsync(Shared.Models.UmbCheckoutConfiguration configuration, CancellationToken cancellationToken = default)
        {
            var incoming = _mapper.Map<Shared.Models.UmbCheckoutConfiguration, UmbCheckoutConfiguration>(configuration);
            if (incoming == null)
            {
                return false;
            }

            using var scope = _scopeProvider.CreateScope();
            var saved = await scope.ExecuteWithContextAsync(async db =>
            {
                var existing = await db.Configurations.SingleOrDefaultAsync(cancellationToken);
                if (existing == null)
                {
                    incoming.Id = 0;
                    incoming.Key = incoming.Key is null || incoming.Key == Guid.Empty ? Guid.NewGuid() : incoming.Key;
                    db.Configurations.Add(incoming);
                    await db.SaveChangesAsync(cancellationToken);
                    return incoming;
                }

                incoming.Id = existing.Id;
                incoming.Key = existing.Key ?? Guid.NewGuid();
                db.Entry(existing).CurrentValues.SetValues(incoming);
                await db.SaveChangesAsync(cancellationToken);
                return existing;
            });

            scope.Notifications.Publish(new OnConfigurationSavedNotification(_mapper.Map<UmbCheckoutConfiguration, Shared.Models.UmbCheckoutConfiguration>(saved)));
            scope.Complete();
            _logger.LogDebug("UmbCheckout configuration saved");
            return true;
        }
    }
}
