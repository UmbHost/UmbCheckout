using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Helpers;
using UmbCheckout.Shared.Models;
using UmbCheckout.Shared.Notifications.Session;
using UmbHost.Licensing.Services;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Cms.Web.Common.Security;

namespace UmbCheckout.Core.Services
{
    /// <summary>
    /// A service to handle the Get, Update and Clearing of the Session: one basket per site root culture and currency
    /// </summary>
    public sealed class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICoreScopeProvider _coreScopeProvider;
        private readonly ILogger<SessionService> _logger;
        private readonly IConfigurationService _configurationService;
        private readonly IBasketContext _basketContext;

        [Obsolete("Use the constructor that takes an IBasketContext. This one builds the basket context from the application's services.")]
        public SessionService(IDataProtectionProvider dataProtectionProvider, IHttpContextAccessor contextAccessor, ILogger<SessionService> logger, IEventAggregator eventAggregator, ICoreScopeProvider coreScopeProvider, IConfigurationService configurationService, LicenseService licenseService, IUmbracoContextAccessor umbracoContextAccessor)
            : this(dataProtectionProvider, contextAccessor, logger, eventAggregator, coreScopeProvider, configurationService, licenseService,
                new UmbracoBasketContext(umbracoContextAccessor, StaticServiceProvider.Instance.GetRequiredService<ICurrencyService>(), configurationService))
        {
        }

        [ActivatorUtilitiesConstructor]
        public SessionService(IDataProtectionProvider dataProtectionProvider, IHttpContextAccessor contextAccessor, ILogger<SessionService> logger, IEventAggregator eventAggregator, ICoreScopeProvider coreScopeProvider, IConfigurationService configurationService, LicenseService licenseService, IBasketContext basketContext)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _contextAccessor = contextAccessor;
            _logger = logger;
            _eventAggregator = eventAggregator;
            _coreScopeProvider = coreScopeProvider;
            _configurationService = configurationService;
            _basketContext = basketContext;
        }

        private HttpContext HttpContext => _contextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext cannot be null");

        /// <summary>
        /// Creates the UmbCheckout Session for one site root culture and currency
        /// </summary>
        private async Task<UmbCheckoutSession> Create(string? siteRootCulture, string? currencyCode)
        {
            var configuration = await _configurationService.GetConfiguration();

            using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
            await _eventAggregator.PublishAsync(new OnSessionCreateStartedNotification());

            var sessionId = HttpContext.Session.Id;
            var session = new UmbCheckoutSession
            {
                SiteRootCulture = siteRootCulture,
                Basket = new Basket { SessionId = sessionId, SiteRootCulture = siteRootCulture, CurrencyCode = currencyCode }
            };

            var encryptedBasket = EncryptionHelper.Encrypt(JsonSerializer.Serialize(session.Basket), _dataProtectionProvider);
            HttpContext.Session.SetObjectAsJson(BasketKeys.Session(siteRootCulture, currencyCode), session);

            scope.Notifications.Publish(new OnSessionCreatedNotification(HttpContext, sessionId, encryptedBasket, configuration));
            return session;
        }

        /// <inheritdoc />
        public async Task<UmbCheckoutSession> Get() => await Get(await _basketContext.GetCurrencyAsync());

        /// <inheritdoc />
        public async Task<UmbCheckoutSession> Get(string? currencyCode) => await Get(_basketContext.SiteRootCulture, currencyCode);

        /// <inheritdoc />
        public async Task<UmbCheckoutSession> Get(string? siteRootCulture, string? currencyCode)
        {
            currencyCode = Normalise(currencyCode);
            var configuration = await _configurationService.GetConfiguration();

            using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
            await _eventAggregator.PublishAsync(new OnSessionGetStartedNotification());

            MigrateLegacyBasket(siteRootCulture, currencyCode);

            var key = BasketKeys.Session(siteRootCulture, currencyCode);
            var session = (HttpContext.Session.Keys.Contains(key) ? HttpContext.Session.GetObjectFromJson<UmbCheckoutSession>(key) : null)
                          ?? await Create(siteRootCulture, currencyCode);

            scope.Notifications.Publish(new OnSessionGetNotification(HttpContext, session, configuration));
            return session;
        }

        /// <summary>
        /// Baskets saved before they were per currency live under the culture-only key: each currency's lines move to their own
        /// basket (a basket already stored under its new key is kept), and lines with no currency are dropped with a warning.
        /// </summary>
        private void MigrateLegacyBasket(string? siteRootCulture, string? currencyCode)
        {
            var legacyKey = BasketKeys.LegacySession(siteRootCulture);
            if (currencyCode == null || !HttpContext.Session.Keys.Contains(legacyKey))
            {
                return;
            }

            var legacy = HttpContext.Session.GetObjectFromJson<UmbCheckoutSession>(legacyKey);
            HttpContext.Session.Remove(legacyKey);
            if (legacy == null)
            {
                return;
            }

            var split = BasketSplitter.ByCurrency(legacy.Basket);
            foreach (var basket in split.Baskets)
            {
                var key = BasketKeys.Session(siteRootCulture, basket.CurrencyCode);
                if (!HttpContext.Session.Keys.Contains(key))
                {
                    HttpContext.Session.SetObjectAsJson(key, new UmbCheckoutSession { SiteRootCulture = siteRootCulture, Basket = basket });
                }
            }

            if (split.Dropped.Count > 0)
            {
                _logger.LogWarning("Dropped {LineCount} basket lines with no currency while splitting the {SiteRootCulture} basket by currency: {ProductKeys}", split.Dropped.Count, siteRootCulture, string.Join(", ", split.Dropped.Select(lineItem => lineItem.Key)));
            }
        }

        /// <inheritdoc />
        public async Task<UmbCheckoutSession> Update(Basket basket)
        {
            var configuration = await _configurationService.GetConfiguration();

            using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
            await _eventAggregator.PublishAsync(new OnSessionUpdateStartedNotification());

            var siteRootCulture = _basketContext.SiteRootCulture;
            basket.SiteRootCulture = siteRootCulture;
            basket.CurrencyCode = Normalise(basket.CurrencyCode) ?? Normalise(await _basketContext.GetCurrencyAsync());
            var session = new UmbCheckoutSession { SiteRootCulture = siteRootCulture, Basket = basket };

            var sessionId = HttpContext.Session.Id;
            HttpContext.Session.SetObjectAsJson(BasketKeys.Session(siteRootCulture, basket.CurrencyCode), session);

            var encryptedBasket = EncryptionHelper.Encrypt(JsonSerializer.Serialize(basket), _dataProtectionProvider);
            scope.Notifications.Publish(new OnSessionUpdatedNotification(HttpContext, sessionId, basket, encryptedBasket, configuration));

            return session;
        }

        /// <inheritdoc />
        public async Task<bool> Clear() => await Clear(await _basketContext.GetCurrencyAsync());

        /// <inheritdoc />
        public async Task<bool> Clear(string? currencyCode) => await Clear(_basketContext.SiteRootCulture, currencyCode);

        /// <inheritdoc />
        public async Task<bool> Clear(string? siteRootCulture, string? currencyCode)
        {
            currencyCode = Normalise(currencyCode);
            var configuration = await _configurationService.GetConfiguration();

            using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
            await _eventAggregator.PublishAsync(new OnSessionClearStartedNotification());

            var sessionId = HttpContext.Session.Id;
            var key = BasketKeys.Session(siteRootCulture, currencyCode);

            // A basket saved before the upgrade (e.g. a checkout started then, paid now) is split first, so its paid lines don't
            // come back from the legacy key on the next read.
            MigrateLegacyBasket(siteRootCulture, currencyCode);

            // Only this basket: the customer's baskets in other currencies, and other packages' session values, are kept.
            HttpContext.Session.Remove(key);
            await HttpContext.Session.CommitAsync();

            scope.Notifications.Publish(new OnSessionClearedNotification(HttpContext, sessionId, configuration)
            {
                SiteRootCulture = siteRootCulture,
                CurrencyCode = currencyCode
            });

            return HttpContext.Session.Keys.Contains(key);
        }

        private static string? Normalise(string? currencyCode)
            => string.IsNullOrWhiteSpace(currencyCode) ? null : currencyCode.Trim().ToUpperInvariant();
    }
}
