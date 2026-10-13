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
using UmbCheckout.Shared.Notifications;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.DependencyInjection;
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
        private readonly IEventMessagesFactory _eventMessagesFactory;
        private readonly IPaidCheckoutSessions _paidCheckoutSessions;

        public SessionService(IDataProtectionProvider dataProtectionProvider, IHttpContextAccessor contextAccessor, ILogger<SessionService> logger, IEventAggregator eventAggregator, ICoreScopeProvider coreScopeProvider, IConfigurationService configurationService, LicenseService licenseService, IBasketContext basketContext,
            IEventMessagesFactory eventMessagesFactory, IPaidCheckoutSessions paidCheckoutSessions)
        {
            _eventMessagesFactory = eventMessagesFactory;
            _paidCheckoutSessions = paidCheckoutSessions;
            _dataProtectionProvider = dataProtectionProvider;
            _contextAccessor = contextAccessor;
            _logger = logger;
            _eventAggregator = eventAggregator;
            _coreScopeProvider = coreScopeProvider;
            _configurationService = configurationService;
            _basketContext = basketContext;
        }

        [Obsolete("Use the constructor that takes an IPaidCheckoutSessions. Scheduled for removal in the next major version.")]
        public SessionService(IDataProtectionProvider dataProtectionProvider, IHttpContextAccessor contextAccessor, ILogger<SessionService> logger, IEventAggregator eventAggregator, ICoreScopeProvider coreScopeProvider, IConfigurationService configurationService, LicenseService licenseService, IBasketContext basketContext,
            IEventMessagesFactory eventMessagesFactory)
            : this(dataProtectionProvider, contextAccessor, logger, eventAggregator, coreScopeProvider, configurationService, licenseService, basketContext,
                eventMessagesFactory, PaidCheckoutSessionsOrNone())
        {
        }

        [Obsolete("Use the constructor that takes an IEventMessagesFactory and an IPaidCheckoutSessions. Scheduled for removal in the next major version.")]
        public SessionService(IDataProtectionProvider dataProtectionProvider, IHttpContextAccessor contextAccessor, ILogger<SessionService> logger, IEventAggregator eventAggregator, ICoreScopeProvider coreScopeProvider, IConfigurationService configurationService, LicenseService licenseService, IBasketContext basketContext)
            : this(dataProtectionProvider, contextAccessor, logger, eventAggregator, coreScopeProvider, configurationService, licenseService, basketContext,
                StaticServiceProvider.Instance.GetRequiredService<IEventMessagesFactory>(), PaidCheckoutSessionsOrNone())
        {
        }

        /// <summary>For the obsolete constructors: the registered store, or "none paid" when there is no container (e.g. built by hand).</summary>
        private static IPaidCheckoutSessions PaidCheckoutSessionsOrNone()
            => (StaticServiceProvider.Instance as IServiceProvider)?.GetService(typeof(IPaidCheckoutSessions)) as IPaidCheckoutSessions ?? new NoPaidCheckoutSessions();

        private HttpContext HttpContext => _contextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext cannot be null");

        /// <summary>For the overloads without a token: the request's own (a basket always belongs to a request).</summary>
        private CancellationToken RequestAborted => _contextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;

        private async Task<UmbCheckoutSession> Create(string? siteRootCulture, string? currencyCode, CancellationToken cancellationToken)
        {
            var configuration = await _configurationService.GetConfigurationAsync(cancellationToken);

            using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
            await _eventAggregator.PublishAsync(new OnSessionCreateStartedNotification(), cancellationToken);

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
        [Obsolete("Use Get(CancellationToken) (the overload with a CancellationToken). Scheduled for removal in the next major version.")]
        public Task<UmbCheckoutSession> Get() => Get(RequestAborted);

        /// <inheritdoc />
        public async Task<UmbCheckoutSession> Get(CancellationToken cancellationToken) => await Get(await _basketContext.GetCurrencyAsync(cancellationToken), cancellationToken);

        /// <inheritdoc />
        [Obsolete("Use Get(string?, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in the next major version.")]
        public Task<UmbCheckoutSession> Get(string? currencyCode) => Get(currencyCode, RequestAborted);

        /// <inheritdoc />
        public Task<UmbCheckoutSession> Get(string? currencyCode, CancellationToken cancellationToken) => Get(_basketContext.SiteRootCulture, currencyCode, cancellationToken);

        /// <inheritdoc />
        [Obsolete("Use Get(string?, string?, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in the next major version.")]
        public Task<UmbCheckoutSession> Get(string? siteRootCulture, string? currencyCode) => Get(siteRootCulture, currencyCode, RequestAborted);

        /// <inheritdoc />
        public async Task<UmbCheckoutSession> Get(string? siteRootCulture, string? currencyCode, CancellationToken cancellationToken)
        {
            var configuration = await _configurationService.GetConfigurationAsync(cancellationToken);

            UmbCheckoutSession session;
            using (var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true))
            {
                await _eventAggregator.PublishAsync(new OnSessionGetStartedNotification(), cancellationToken);

                MigrateLegacyBasket(siteRootCulture, currencyCode);

                var key = BasketKeys.Session(siteRootCulture, currencyCode);
                session = (HttpContext.Session.Keys.Contains(key) ? HttpContext.Session.GetObjectFromJson<UmbCheckoutSession>(key) : null)
                          ?? await Create(siteRootCulture, currencyCode, cancellationToken);

                scope.Notifications.Publish(new OnSessionGetNotification(HttpContext, session, configuration));
            }

            // After the scope: its notifications (the add-on's cookie and database basket restore) have replaced the basket by now.
            session = await ClearIfPaidAsync(session, siteRootCulture, currencyCode, cancellationToken);

            if (!UmbCheckoutHooks.LoadedSuppressed)
            {
                await _eventAggregator.PublishAsync(new OnSessionLoadedNotification(session, _eventMessagesFactory.Get()), cancellationToken);
            }
            return session;
        }

        /// <summary>
        /// A basket whose checkout session the payment provider has reported paid (webhook) is cleared: the customer paid but never
        /// returned to the success page. Clearing goes through <see cref="Clear(string?, string?)"/>, so the add-on's cookie and
        /// database copies go too. Only baskets that reached checkout carry a session id, so other reads make no lookup.
        /// </summary>
        private async Task<UmbCheckoutSession> ClearIfPaidAsync(UmbCheckoutSession session, string? siteRootCulture, string? currencyCode, CancellationToken cancellationToken)
        {
            var checkoutSessionId = session.Basket.CheckoutSessionId;
            if (string.IsNullOrEmpty(checkoutSessionId))
            {
                return session;
            }

            // An unpaid answer holds for the rest of the request: a page reads its basket several times (page, partials).
            var unpaidKey = UnpaidCheckoutSessionItem + checkoutSessionId;
            if (HttpContext.Items.ContainsKey(unpaidKey))
            {
                return session;
            }

            try
            {
                if (!await _paidCheckoutSessions.IsPaidAsync(checkoutSessionId, cancellationToken))
                {
                    HttpContext.Items[unpaidKey] = true;
                    return session;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // The basket stays until a later read can check it; the read itself mustn't fail.
                _logger.LogWarning(ex, "Could not check whether checkout session {CheckoutSessionId} was paid", checkoutSessionId);
                return session;
            }

            await Clear(siteRootCulture, currencyCode, cancellationToken);
            await _eventAggregator.PublishAsync(new OnPaidBasketClearedNotification(session.Basket, checkoutSessionId), cancellationToken);
            _logger.LogInformation("Cleared a basket paid with checkout session {CheckoutSessionId}", checkoutSessionId);

            var empty = new UmbCheckoutSession
            {
                SiteRootCulture = siteRootCulture,
                Basket = new Basket { SessionId = HttpContext.Session.Id, SiteRootCulture = siteRootCulture, CurrencyCode = currencyCode }
            };
            HttpContext.Session.SetObjectAsJson(BasketKeys.Session(siteRootCulture, currencyCode), empty);
            return empty;
        }

        private const string UnpaidCheckoutSessionItem = "UmbCheckout.UnpaidCheckoutSession:";

        /// <summary>
        /// A basket whose lines change after checkout is no longer the basket that checkout session pays for: it forgets the session,
        /// so a payment recorded later (delayed methods such as Bacs or SEPA arrive days later) doesn't clear the new lines. Setting
        /// the session id at checkout (lines unchanged) keeps it.
        /// </summary>
        private static void ForgetCheckoutSessionIfChanged(Basket basket, Basket? stored)
        {
            if (string.IsNullOrEmpty(basket.CheckoutSessionId) || stored == null || basket.CheckoutSessionId != stored.CheckoutSessionId)
            {
                return;
            }

            static string Lines(Basket b) => string.Join(";", b.LineItems.OrderBy(l => l.Key).Select(l => $"{l.Key}x{l.Quantity}"));
            if (Lines(basket) != Lines(stored))
            {
                basket.CheckoutSessionId = null;
            }
        }

        /// <summary>
        /// Baskets saved before they were per currency live under the culture-only key: each currency's lines move to their own basket.
        /// </summary>
        private void MigrateLegacyBasket(string? siteRootCulture, string? currencyCode)
        {
            var legacyKey = BasketKeys.LegacySession(siteRootCulture);
            if (string.IsNullOrWhiteSpace(currencyCode) || !HttpContext.Session.Keys.Contains(legacyKey))
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
        [Obsolete("Use Update(Basket, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in the next major version.")]
        public Task<UmbCheckoutSession> Update(Basket basket) => Update(basket, RequestAborted);

        /// <inheritdoc />
        public async Task<UmbCheckoutSession> Update(Basket basket, CancellationToken cancellationToken)
        {
            var configuration = await _configurationService.GetConfigurationAsync(cancellationToken);

            using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
            await _eventAggregator.PublishAsync(new OnSessionUpdateStartedNotification(), cancellationToken);

            var siteRootCulture = _basketContext.SiteRootCulture;
            basket.SiteRootCulture = siteRootCulture;
            basket.CurrencyCode ??= await _basketContext.GetCurrencyAsync(cancellationToken);

            var key = BasketKeys.Session(siteRootCulture, basket.CurrencyCode);
            var stored = HttpContext.Session.Keys.Contains(key) ? HttpContext.Session.GetObjectFromJson<UmbCheckoutSession>(key) : null;
            ForgetCheckoutSessionIfChanged(basket, stored?.Basket);
            if (await _eventAggregator.PublishCancellingAsync(new OnSessionSavingNotification(basket, stored?.Basket, _eventMessagesFactory.Get()), _eventMessagesFactory.Get()))
            {
                return stored ?? new UmbCheckoutSession { SiteRootCulture = siteRootCulture, Basket = new Basket { SiteRootCulture = siteRootCulture, CurrencyCode = basket.CurrencyCode } };
            }

            var session = new UmbCheckoutSession { SiteRootCulture = siteRootCulture, Basket = basket };

            var sessionId = HttpContext.Session.Id;
            HttpContext.Session.SetObjectAsJson(key, session);

            var encryptedBasket = EncryptionHelper.Encrypt(JsonSerializer.Serialize(basket), _dataProtectionProvider);
            scope.Notifications.Publish(new OnSessionUpdatedNotification(HttpContext, sessionId, basket, encryptedBasket, configuration));

            return session;
        }

        /// <inheritdoc />
        [Obsolete("Use Clear(CancellationToken) (the overload with a CancellationToken). Scheduled for removal in the next major version.")]
        public Task<bool> Clear() => Clear(RequestAborted);

        /// <inheritdoc />
        public async Task<bool> Clear(CancellationToken cancellationToken) => await Clear(await _basketContext.GetCurrencyAsync(cancellationToken), cancellationToken);

        /// <inheritdoc />
        [Obsolete("Use Clear(string?, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in the next major version.")]
        public Task<bool> Clear(string? currencyCode) => Clear(currencyCode, RequestAborted);

        /// <inheritdoc />
        public Task<bool> Clear(string? currencyCode, CancellationToken cancellationToken) => Clear(_basketContext.SiteRootCulture, currencyCode, cancellationToken);

        /// <inheritdoc />
        [Obsolete("Use Clear(string?, string?, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in the next major version.")]
        public Task<bool> Clear(string? siteRootCulture, string? currencyCode) => Clear(siteRootCulture, currencyCode, RequestAborted);

        /// <inheritdoc />
        /// <remarks>The token stops the clear before it starts; once the basket is removed, the session is committed regardless.</remarks>
        public async Task<bool> Clear(string? siteRootCulture, string? currencyCode, CancellationToken cancellationToken)
        {
            var configuration = await _configurationService.GetConfigurationAsync(cancellationToken);

            using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
            await _eventAggregator.PublishAsync(new OnSessionClearStartedNotification(), cancellationToken);

            var sessionId = HttpContext.Session.Id;
            var key = BasketKeys.Session(siteRootCulture, currencyCode);

            HttpContext.Session.Remove(key);
            await HttpContext.Session.CommitAsync();

            scope.Notifications.Publish(new OnSessionClearedNotification(HttpContext, sessionId, configuration)
            {
                SiteRootCulture = siteRootCulture,
                CurrencyCode = currencyCode
            });

            return HttpContext.Session.Keys.Contains(key);
        }
    }
}
