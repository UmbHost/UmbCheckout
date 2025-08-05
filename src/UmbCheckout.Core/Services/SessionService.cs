using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared;
using UmbCheckout.Shared.Helpers;
using UmbCheckout.Shared.Models;
using UmbCheckout.Shared.Notifications.Session;
using UmbHost.Licencing.Services;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;

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
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public SessionService(IDataProtectionProvider dataProtectionProvider, IHttpContextAccessor contextAccessor, ILogger<SessionService> logger, IEventAggregator eventAggregator, ICoreScopeProvider coreScopeProvider, IConfigurationService configurationService, LicenceService licenseService, IUmbracoContextAccessor umbracoContextAccessor)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _contextAccessor = contextAccessor;
            _logger = logger;
            _eventAggregator = eventAggregator;
            _coreScopeProvider = coreScopeProvider;
            _configurationService = configurationService;
            _umbracoContextAccessor = umbracoContextAccessor;
            licenseService.RunLicenceCheck();
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

                var hasUmbracoContext = _umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext);
                if (umbracoContext == null || !hasUmbracoContext)
                {
                    throw new InvalidOperationException("Umbraco Context cannot be null");
                }

                var configuration = await _configurationService.GetConfiguration();

                using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                await _eventAggregator.PublishAsync(new OnSessionCreateStartedNotification());

                var sessionId = _contextAccessor.HttpContext.Session.Id;
                var siteRootCulture = umbracoContext.PublishedRequest?.PublishedContent?.Root()?.GetCultureFromDomains();
                var session = new UmbCheckoutSession
                {
                    SiteRootCulture = siteRootCulture,
                    Basket = new Basket
                    {
                        SessionId = sessionId,
                        SiteRootCulture = siteRootCulture
                    }
                };

                var encryptedBasket = EncryptionHelper.Encrypt(JsonSerializer.Serialize(session.Basket), _dataProtectionProvider);

                _contextAccessor.HttpContext.Session.SetObjectAsJson(Consts.SessionKey + siteRootCulture, session);

                scope.Notifications.Publish(new OnSessionCreatedNotification(_contextAccessor.HttpContext, sessionId, encryptedBasket, configuration));
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

                var hasUmbracoContext = _umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext);
                if ( umbracoContext == null || !hasUmbracoContext)
                {
                    throw new InvalidOperationException("Umbraco Context cannot be null");
                }

                var configuration = await _configurationService.GetConfiguration();

                using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                await _eventAggregator.PublishAsync(new OnSessionGetStartedNotification());

                var siteRootCulture = umbracoContext.PublishedRequest?.PublishedContent?.Root()?.GetCultureFromDomains();

                var session = (_contextAccessor.HttpContext.Session.Keys.Contains(Consts.SessionKey + siteRootCulture) ? _contextAccessor.HttpContext.Session.GetObjectFromJson<UmbCheckoutSession>(Consts.SessionKey + siteRootCulture) :
                    await Create()) ?? await Create();

                scope.Notifications.Publish(new OnSessionGetNotification(_contextAccessor.HttpContext, session, configuration));

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

                var hasUmbracoContext = _umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext);
                if (umbracoContext == null || !hasUmbracoContext)
                {
                    throw new InvalidOperationException("Umbraco Context cannot be null");
                }

                var configuration = await _configurationService.GetConfiguration();

                using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                await _eventAggregator.PublishAsync(new OnSessionUpdateStartedNotification());

                var session = await Get();
                var siteRootCulture = umbracoContext.PublishedRequest?.PublishedContent?.Root()?.GetCultureFromDomains();
                basket.SiteRootCulture = siteRootCulture;
                session.Basket = basket;

                var sessionId = _contextAccessor.HttpContext.Session.Id;

                _contextAccessor.HttpContext.Session.SetObjectAsJson(Consts.SessionKey + siteRootCulture, session);

                var encryptedBasket = EncryptionHelper.Encrypt(JsonSerializer.Serialize(basket), _dataProtectionProvider);
                scope.Notifications.Publish(new OnSessionUpdatedNotification(_contextAccessor.HttpContext, sessionId, basket, encryptedBasket, configuration));

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

                var hasUmbracoContext = _umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext);
                if (umbracoContext == null || !hasUmbracoContext)
                {
                    throw new InvalidOperationException("Umbraco Context cannot be null");
                }

                var configuration = await _configurationService.GetConfiguration();

                using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                await _eventAggregator.PublishAsync(new OnSessionClearStartedNotification());

                var sessionId = _contextAccessor.HttpContext.Session.Id;
                var siteRootCulture = umbracoContext.PublishedRequest?.PublishedContent?.Root()?.GetCultureFromDomains();

                _contextAccessor.HttpContext.Session.Clear();

                await _contextAccessor.HttpContext.Session.CommitAsync();

                scope.Notifications.Publish(new OnSessionClearedNotification(_contextAccessor.HttpContext, sessionId, configuration));

                return _contextAccessor.HttpContext.Session.Keys.Contains(Consts.SessionKey + siteRootCulture);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
