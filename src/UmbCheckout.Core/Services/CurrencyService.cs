using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared;
using UmbCheckout.Shared.Extensions;
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

        /// <summary>
        /// Gets the cultures currency or global currency
        /// </summary>
        /// <param name="currencyNodeKey">The content item which contains the currency code for the current culture</param>
        /// <returns>The currency code for the current culture or global currency</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<string> GetCurrencyAsync(Guid? currencyNodeKey)
        {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
