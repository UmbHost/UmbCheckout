using System.ComponentModel;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UmbCheckout.Core.Helpers;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared;
using UmbCheckout.Shared.Helpers;
using UmbCheckout.Shared.Models;
using UmbCheckout.Shared.Notifications.Session;
using UmbHost.Licensing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Web.Common.Security;

namespace UmbCheckout.Core.Services
{
    /// <summary>
    /// A service to handle the Get, Update and Clearing of the Session
    /// </summary>
    [LicenseProvider(typeof(UmbLicensingProvider))]
    internal class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICoreScopeProvider _coreScopeProvider;
        private readonly ILogger<SessionService> _logger;
        private readonly IDatabaseService _databaseService;
        private readonly IConfigurationService _configurationService;
        private string? _sessionId = string.Empty;
        private readonly bool _licenseIsValid;

        public SessionService(IDataProtectionProvider dataProtectionProvider, IHttpContextAccessor contextAccessor, ILogger<SessionService> logger, IEventAggregator eventAggregator, ICoreScopeProvider coreScopeProvider, IConfigurationService configurationService, IDatabaseService databaseService)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _contextAccessor = contextAccessor;
            _logger = logger;
            _eventAggregator = eventAggregator;
            _coreScopeProvider = coreScopeProvider;
            _configurationService = configurationService;
            _databaseService = databaseService;
            _licenseIsValid = LicenseManager.IsValid(typeof(SessionService));
        }

        /// <summary>
        /// Creates the UmbCheckout Session
        /// </summary>
        /// <returns>The current UmbCheckout Session</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task<UmbCheckoutSession> Create()
        {
            try
            {
                if (_contextAccessor.HttpContext == null)
                    throw new InvalidOperationException("HttpContext cannot be null");

                var configuration = await _configurationService.GetConfiguration();

                using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                await _eventAggregator.PublishAsync(new OnSessionCreateStartedNotification());

                var encryptedSessionId = EncryptionHelper.Encrypt(Guid.NewGuid().ToString(), _dataProtectionProvider);
                _sessionId = encryptedSessionId;
                var session = new UmbCheckoutSession
                {
                    Basket = new Basket
                    {
                        SessionId = _sessionId
                    }
                };

                CookieHelper.Set(_contextAccessor.HttpContext, Consts.SessionKey, encryptedSessionId);
                _contextAccessor.HttpContext.Session.SetObjectAsJson(encryptedSessionId, session);

                scope.Notifications.Publish(new OnSessionCreatedNotification(_contextAccessor.HttpContext, Consts.SessionKey, encryptedSessionId, configuration));
                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<UmbCheckoutSession> Get()
        {
            try
            {
                if (_contextAccessor.HttpContext == null)
                    throw new InvalidOperationException("HttpContext cannot be null");

                var configuration = await _configurationService.GetConfiguration();

                using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                await _eventAggregator.PublishAsync(new OnSessionCreateStartedNotification());

                if (configuration != null)
                {
                    _sessionId = CookieHelper.Get(_contextAccessor.HttpContext, Consts.SessionKey);
                }

                var session = (string.IsNullOrEmpty(_sessionId) ? await Create() : _contextAccessor.HttpContext.Session.GetObjectFromJson<UmbCheckoutSession>(_sessionId)) ??
                              await Create();

                if (configuration is { StoreBasketInCookie: true })
                {
                    var encryptedBasketCookie = CookieHelper.Get(_contextAccessor.HttpContext, Consts.SessionKey + "_Basket");
                    if (encryptedBasketCookie != null)
                    {
                        var basketCookie = EncryptionHelper.Decrypt(encryptedBasketCookie, _dataProtectionProvider);
                        var basket = JsonSerializer.Deserialize<Basket>(basketCookie);
                        if (basket != null)
                        {
                            session.Basket = basket;
                            _contextAccessor.HttpContext.Session.SetObjectAsJson(_sessionId!, session);
                        }
                    }
                    else
                    {
                        if (configuration.StoreBasketInDatabase && !string.IsNullOrEmpty(_sessionId) && _licenseIsValid)
                        {
                            var basket = await _databaseService.GetBasket(_sessionId);
                            if (basket != null)
                            {
                                session.Basket = basket;
                            }
                        }
                    }
                }

                scope.Notifications.Publish(new OnSessionGetNotification(_contextAccessor.HttpContext, _sessionId, session.Basket, configuration));

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<UmbCheckoutSession> Update(Basket basket)
        {
            try
            {
                if (_contextAccessor.HttpContext == null)
                    throw new InvalidOperationException("HttpContext cannot be null");

                var configuration = await _configurationService.GetConfiguration();

                using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                await _eventAggregator.PublishAsync(new OnSessionUpdateStartedNotification());

                var session = await Get();

                session.Basket = basket;

                _contextAccessor.HttpContext.Session.SetObjectAsJson(_sessionId!, session);

                var encryptedBasket = EncryptionHelper.Encrypt(JsonSerializer.Serialize(basket), _dataProtectionProvider);
                scope.Notifications.Publish(new OnSessionUpdatedNotification(_contextAccessor.HttpContext, Consts.SessionKey + "_Basket", basket, encryptedBasket, configuration));

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task Clear()
        {
            try
            {
                if (_contextAccessor.HttpContext == null)
                    throw new InvalidOperationException("HttpContext cannot be null");

                var configuration = await _configurationService.GetConfiguration();

                using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                await _eventAggregator.PublishAsync(new OnSessionClearStartedNotification());

                var sessionId = CookieHelper.Get(_contextAccessor.HttpContext, Consts.SessionKey);

                scope.Notifications.Publish(new OnSessionClearedNotification(_contextAccessor.HttpContext, sessionId, configuration));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
