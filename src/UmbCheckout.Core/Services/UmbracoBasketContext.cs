using UmbCheckout.Core.Interfaces;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace UmbCheckout.Core.Services
{
    /// <inheritdoc />
    internal class UmbracoBasketContext : IBasketContext
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ICurrencyService _currencyService;
        private readonly IConfigurationService _configurationService;

        public UmbracoBasketContext(IUmbracoContextAccessor umbracoContextAccessor, ICurrencyService currencyService, IConfigurationService configurationService)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _currencyService = currencyService;
            _configurationService = configurationService;
        }

        private IPublishedContent? CurrentPage
            => _umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext) ? umbracoContext.PublishedRequest?.PublishedContent : null;

        /// <inheritdoc />
        public string? SiteRootCulture => CurrentPage?.Root()?.GetCultureFromDomains();

        /// <inheritdoc />
        [Obsolete("Use GetCurrencyAsync(CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        public Task<string?> GetCurrencyAsync() => GetCurrencyAsync(CancellationToken.None);

        /// <inheritdoc />
        public async Task<string?> GetCurrencyAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var page = CurrentPage;
            if (page != null)
            {
                return await _currencyService.GetCurrencyForContentAsync(page, cancellationToken);
            }

            var configuration = await _configurationService.GetConfiguration();
            return string.IsNullOrWhiteSpace(configuration?.CurrencyCode) ? null : configuration.CurrencyCode.Trim().ToUpperInvariant();
        }
    }
}
