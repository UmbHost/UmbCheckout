using Microsoft.Extensions.Logging;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Pocos;
using UmbCheckout.Shared.Models;
using UmbHost.Licencing.Models;
using UmbHost.Licencing.Services;
using Umbraco.Cms.Infrastructure.Scoping;

namespace UmbCheckout.Core.Services
{
    /// <summary>
    /// A service which handles storing of the Basket in the database
    /// </summary>
    public class DatabaseService : IDatabaseService
    {
        private readonly ILogger<DatabaseService> _logger;
        private readonly IScopeProvider _scopeProvider;
        private readonly IDatabaseMapperService _databaseMapperService;

        public DatabaseService(ILogger<DatabaseService> logger, IScopeProvider scopeProvider, LicenceService licenseService, IDatabaseMapperService databaseMapperService)
        {
            _logger = logger;
            _scopeProvider = scopeProvider;
            _databaseMapperService = databaseMapperService;
            licenseService.RunLicenceCheck();
        }

        /// <inheritdoc />
        public async Task<Basket?> GetBasket(string sessionId)
        {
            try
            {
                if (!UmbCheckoutSettings.IsLicenced)
                {
                    return null;
                }
                using var dbScope = _scopeProvider.CreateScope(autoComplete: true);
                var db = dbScope.Database;
                var existingBasket = await db.QueryAsync<UmbCheckoutBasket>().Where(x => x.SessionId == sessionId).SingleOrDefault();
                if (existingBasket != null)
                {
                    var basket = _databaseMapperService.ToBasket(existingBasket);
                    return basket;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
