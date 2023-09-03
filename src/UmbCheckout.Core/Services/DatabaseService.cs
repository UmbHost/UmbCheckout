using Microsoft.Extensions.Logging;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Pocos;
using UmbCheckout.Shared.Models;
using UmbHost.Licensing.Models;
using UmbHost.Licensing.Services;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Infrastructure.Scoping;

namespace UmbCheckout.Core.Services
{
    /// <summary>
    /// A service which handles storing of the Basket in the database
    /// </summary>
    internal class DatabaseService : IDatabaseService
    {
        private readonly ILogger<DatabaseService> _logger;
        private readonly IScopeProvider _scopeProvider;
        private readonly IUmbracoMapper _mapper;

        public DatabaseService(ILogger<DatabaseService> logger, IScopeProvider scopeProvider, IUmbracoMapper mapper, LicenseService licenseService)
        {
            _logger = logger;
            _scopeProvider = scopeProvider;
            _mapper = mapper;
            licenseService.RunLicenseCheck();
        }

        /// <inheritdoc />
        public async Task<Basket?> GetBasket(string sessionId)
        {
            try
            {
                if (!UmbCheckoutSettings.IsLicensed)
                {
                    return null;
                }
                using var dbScope = _scopeProvider.CreateScope(autoComplete: true);
                var db = dbScope.Database;
                var existingBasket = await db.QueryAsync<UmbCheckoutBasket>().Where(x => x.SessionId == sessionId).SingleOrDefault();
                if (existingBasket != null)
                {
                    return _mapper.Map<UmbCheckoutBasket, Basket>(existingBasket);
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
