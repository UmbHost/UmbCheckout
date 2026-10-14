using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared;
using UmbCheckout.Shared.Extensions;
using Umbraco.Cms.Core.Models.PublishedContent;
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

        public CurrencyService(IConfigurationService configurationService, ILogger<CurrencyService> logger, IUmbracoContextAccessor umbracoContextAccessor)
        {
            _configurationService = configurationService;
            _logger = logger;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>1 once "no currency is configured" has been logged in this process (it is asked on every product render).</summary>
        private static int _noCurrencyLogged;

        /// <summary>1 once "the currency came from the UI culture" has been logged in this process.</summary>
        private static int _cultureCurrencyLogged;

        /// <summary>Lets tests see the once-per-process logs again.</summary>
        internal static void ResetOncePerProcessLogs()
        {
            Interlocked.Exchange(ref _noCurrencyLogged, 0);
            Interlocked.Exchange(ref _cultureCurrencyLogged, 0);
        }

        /// <inheritdoc />
        [Obsolete("Use GetCurrencyForContentAsync(IPublishedContent, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        public Task<string?> GetCurrencyForContentAsync(IPublishedContent? content) => GetCurrencyForContentAsync(content, CancellationToken.None);

        /// <inheritdoc />
        public async Task<string?> GetCurrencyForContentAsync(IPublishedContent? content, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (content is null)
            {
                return null;
            }

            var root = content.Root() ?? content;
            var siteCurrency = root.GetProperty(Consts.PropertyAlias.CurrencyCode)?.GetValue() as string;
            if (!string.IsNullOrWhiteSpace(siteCurrency))
            {
                return siteCurrency.Trim().ToUpperInvariant();
            }

            var configuration = await _configurationService.GetConfiguration();
            if (!string.IsNullOrWhiteSpace(configuration?.CurrencyCode))
            {
                return configuration.CurrencyCode.Trim().ToUpperInvariant();
            }

            // Sites upgraded from an earlier 13.x relied on this last step of the old chain (GetCurrencyAsync); refusing every add would break them.
            var cultureCurrency = UICultureCurrency();
            if (cultureCurrency != null)
            {
                if (Interlocked.Exchange(ref _cultureCurrencyLogged, 1) == 0)
                {
                    _logger.LogWarning("No currency is configured, so products are sold in {CurrencyCode}, the currency of the current UI culture {Culture}. Set {PropertyAlias} on the site root or the currency in UmbCheckout Configuration; this is logged once per process", cultureCurrency, CultureInfo.CurrentUICulture.Name, Consts.PropertyAlias.CurrencyCode);
                }

                return cultureCurrency;
            }

            if (Interlocked.Exchange(ref _noCurrencyLogged, 1) == 0)
            {
                _logger.LogError("No currency is configured for content {ContentKey}: set {PropertyAlias} on its site root or the currency in UmbCheckout Configuration. Products can't be added to the basket until one is set; this is logged once per process", content.Key, Consts.PropertyAlias.CurrencyCode);
            }

            return null;
        }

        /// <summary>The ISO currency of the current UI culture's region when shaped like a code (three ASCII letters); null for a culture with no region, such as the invariant culture.</summary>
        private static string? UICultureCurrency()
        {
            var culture = CultureInfo.CurrentUICulture;
            if (string.IsNullOrEmpty(culture.Name) || culture.IsNeutralCulture)
            {
                return null;
            }

            string code;
            try
            {
                code = new RegionInfo(culture.Name).ISOCurrencySymbol;
            }
            catch (ArgumentException)
            {
                return null;
            }

            return code is { Length: 3 } && code.All(c => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z')
                ? code.ToUpperInvariant()
                : null;
        }

        /// <summary>
        /// Gets the cultures currency or global currency
        /// </summary>
        /// <param name="currencyNodeKey">The content item which contains the currency code for the current culture</param>
        /// <returns>The currency code for the current culture or global currency</returns>
        /// <exception cref="InvalidOperationException"></exception>
        [Obsolete("Use GetCurrencyAsync(Guid?, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        public Task<string> GetCurrencyAsync(Guid? currencyNodeKey) => GetCurrencyAsync(currencyNodeKey, CancellationToken.None);

        /// <inheritdoc />
        public async Task<string> GetCurrencyAsync(Guid? currencyNodeKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var configuration = await _configurationService.GetConfiguration();
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
                    && umbracoContext.PublishedRequest?.PublishedContent?.Root() is { } requestRoot
                    && requestRoot.HasProperty(Consts.PropertyAlias.CurrencyCode)
                    && requestRoot.HasValue(Consts.PropertyAlias.CurrencyCode))
                {
                    stringBuilder.Append(requestRoot.Value<string>(Consts.PropertyAlias.CurrencyCode)?.ToUpper());
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Working out the currency (currency node {CurrencyNodeKey}) failed", currencyNodeKey);
                throw;
            }
        }
    }
}
