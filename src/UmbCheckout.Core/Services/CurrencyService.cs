using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared;
using UmbCheckout.Shared.Extensions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace UmbCheckout.Core.Services
{
    /// <summary>
    /// A service to handle the Get of the Currency
    /// </summary>
    internal class CurrencyService : ICurrencyService
    {
        private readonly IConfigurationService _configurationService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ILogger<CurrencyService> _logger;
        private readonly IDocumentNavigationQueryService _navigation;
        private readonly IPublishedContentCache _contentCache;

        public CurrencyService(IConfigurationService configurationService, ILogger<CurrencyService> logger, IUmbracoContextAccessor umbracoContextAccessor, IDocumentNavigationQueryService navigation, IPublishedContentCache contentCache)
        {
            _configurationService = configurationService;
            _logger = logger;
            _umbracoContextAccessor = umbracoContextAccessor;
            _navigation = navigation;
            _contentCache = contentCache;
        }

        /// <inheritdoc />
        public Task<string?> GetCurrencyForContentAsync(IPublishedContent content) => GetCurrencyForContentAsync(content, CancellationToken.None);

        /// <inheritdoc />
        public async Task<string?> GetCurrencyForContentAsync(IPublishedContent content, CancellationToken cancellationToken)
        {
            var root = await GetRootAsync(content);
            var siteCurrency = root?.GetProperty(Consts.PropertyAlias.CurrencyCode)?.GetValue() as string;
            if (!string.IsNullOrWhiteSpace(siteCurrency))
            {
                return siteCurrency.Trim().ToUpperInvariant();
            }

            var configuration = await _configurationService.GetConfigurationAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(configuration?.CurrencyCode))
            {
                return configuration.CurrencyCode.Trim().ToUpperInvariant();
            }

            _logger.LogError("No currency is configured for content {ContentKey}: set {PropertyAlias} on its site root or the currency in UmbCheckout Configuration", content.Key, Consts.PropertyAlias.CurrencyCode);
            return null;
        }

        private async Task<IPublishedContent?> GetRootAsync(IPublishedContent content)
        {
            var key = content.Key;
            while (_navigation.TryGetParentKey(key, out var parentKey) && parentKey.HasValue)
            {
                key = parentKey.Value;
            }

            return key == content.Key ? content : await _contentCache.GetByIdAsync(key);
        }

        /// <summary>
        /// Gets the cultures currency or global currency
        /// </summary>
        /// <param name="currencyNodeKey">The content item which contains the currency code for the current culture</param>
        /// <returns>The currency code for the current culture or global currency</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Task<string> GetCurrencyAsync(Guid? currencyNodeKey) => GetCurrencyAsync(currencyNodeKey, CancellationToken.None);

        /// <inheritdoc />
        public async Task<string> GetCurrencyAsync(Guid? currencyNodeKey, CancellationToken cancellationToken)
        {
            try
            {
                var configuration = await _configurationService.GetConfigurationAsync(cancellationToken);
                var stringBuilder = new StringBuilder();

                if (currencyNodeKey is not null)
                {
                    if (!_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
                        throw new InvalidOperationException(
                            "The node for the provided currency node id cannot be found.");

                    var currencyNode = umbracoContext.Content?.GetById(currencyNodeKey.Value);
                    if (currencyNode != null)
                    {
                        stringBuilder.Append(currencyNode.Value<string>(Consts.PropertyAlias.CurrencyCode)?.ToUpper());
                    }
                }
                else if (_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext)
                    && umbracoContext.PublishedRequest != null
                    && umbracoContext.PublishedRequest.PublishedContent != null
                    && umbracoContext.PublishedRequest.PublishedContent.Root() != null
                    && umbracoContext.PublishedRequest.PublishedContent.Root()!.HasProperty(Consts.PropertyAlias.CurrencyCode)
                    && umbracoContext.PublishedRequest.PublishedContent.Root()!.HasValue(Consts.PropertyAlias.CurrencyCode))
                {
                    stringBuilder.Append(umbracoContext.PublishedRequest.PublishedContent.Root()!.Value<string>(Consts.PropertyAlias.CurrencyCode)?.ToUpper());
                }
                else if (configuration != null && !string.IsNullOrEmpty(configuration.CurrencyCode))
                {
                    stringBuilder.Append(configuration.CurrencyCode.ToUpper());
                }
                else
                {
                    stringBuilder.Append(CultureInfo.CurrentUICulture.GetISOCurrencySymbol());
                }

                return stringBuilder.ToString();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Working out the currency (currency node {CurrencyNodeKey}) failed", currencyNodeKey);
                throw;
            }
        }
    }
}
