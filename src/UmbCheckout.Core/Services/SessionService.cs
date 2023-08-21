using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UmbCheckout.Core.Helpers;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared;
using UmbCheckout.Shared.Models;
using UmbCheckout.Shared.Notifications.Session;
using UmbHost.Licensing.Helpers;
using UmbHost.Licensing.Services;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Web.Common.Security;

namespace UmbCheckout.Core.Services
{
    /// <summary>
    /// A service to handle the Get, Update and Clearing of the Session
    /// </summary>
    public sealed class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICoreScopeProvider _coreScopeProvider;
        private readonly ILogger<SessionService> _logger;
        private readonly IConfigurationService _configurationService;

        public SessionService(IDataProtectionProvider dataProtectionProvider, IHttpContextAccessor contextAccessor, ILogger<SessionService> logger, IEventAggregator eventAggregator, ICoreScopeProvider coreScopeProvider, IConfigurationService configurationService, LicenseService licenseService)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _contextAccessor = contextAccessor;
            _logger = logger;
            _eventAggregator = eventAggregator;
            _coreScopeProvider = coreScopeProvider;
            _configurationService = configurationService;
            licenseService.RunLicenseCheck();
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

                var sessionId = _contextAccessor.HttpContext.Session.Id;
                var session = new UmbCheckoutSession
                {
                    Basket = new Basket
                    {
                        SessionId = sessionId
                    }
                };

                var encryptedBasket = EncryptionHelper.Encrypt(JsonSerializer.Serialize(session.Basket), _dataProtectionProvider);

                _contextAccessor.HttpContext.Session.SetObjectAsJson(Consts.SessionKey, session);

                scope.Notifications.Publish(new OnSessionCreatedNotification(_contextAccessor.HttpContext, Consts.SessionKey, encryptedBasket, configuration));
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

                var session = (_contextAccessor.HttpContext.Session.Keys.Contains(Consts.SessionKey) ? _contextAccessor.HttpContext.Session.GetObjectFromJson<UmbCheckoutSession>(Consts.SessionKey) :
                    await Create()) ?? await Create();

                if (configuration is { StoreBasketInCookie: true })
                {
                    var encryptedBasketCookie = CookieHelper.Get(_contextAccessor.HttpContext, Consts.SessionBasketKey);
                    if (encryptedBasketCookie != null)
                    {
                        var basketCookie = EncryptionHelper.Decrypt(encryptedBasketCookie, _dataProtectionProvider);
                        var basket = JsonSerializer.Deserialize<Basket>(basketCookie);
                        if (basket != null)
                        {
                            session.Basket = basket;
                            _contextAccessor.HttpContext.Session.SetObjectAsJson(Consts.SessionKey, session);
                        }
                    }
                }

                scope.Notifications.Publish(new OnSessionGetNotification(_contextAccessor.HttpContext, Consts.SessionBasketKey, session.Basket, configuration));

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

                _contextAccessor.HttpContext.Session.SetObjectAsJson(Consts.SessionKey, session);

                var encryptedBasket = EncryptionHelper.Encrypt(JsonSerializer.Serialize(basket), _dataProtectionProvider);
                scope.Notifications.Publish(new OnSessionUpdatedNotification(_contextAccessor.HttpContext, Consts.SessionKey, basket, encryptedBasket, configuration));

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> Clear()
        {
            try
            {
                if (_contextAccessor.HttpContext == null)
                    throw new InvalidOperationException("HttpContext cannot be null");

                var configuration = await _configurationService.GetConfiguration();

                using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                await _eventAggregator.PublishAsync(new OnSessionClearStartedNotification());

                var sessionId = _contextAccessor.HttpContext.Session.Id;

                _contextAccessor.HttpContext.Session.Clear();

                await _contextAccessor.HttpContext.Session.CommitAsync();

                scope.Notifications.Publish(new OnSessionClearedNotification(_contextAccessor.HttpContext, sessionId, configuration));

                return _contextAccessor.HttpContext.Session.Keys.Contains(Consts.SessionKey);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
