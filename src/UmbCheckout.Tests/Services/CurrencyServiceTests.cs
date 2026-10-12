using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Services;
using UmbCheckout.Shared;
using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    /// <summary>The charged currency is decided on the server, never posted by the browser.</summary>
    public class CurrencyServiceTests
    {
        private readonly Mock<IDocumentNavigationQueryService> _navigation = new();
        private readonly Mock<IPublishedContentCache> _cache = new();
        private readonly Mock<IConfigurationService> _configuration = new();

        private CurrencyService Service() => new(_configuration.Object, NullLogger<CurrencyService>.Instance, Mock.Of<IUmbracoContextAccessor>(), _navigation.Object, _cache.Object);

        private static IPublishedContent Content(Guid key, string? currencyCode = null)
        {
            var content = new Mock<IPublishedContent>();
            content.Setup(c => c.Key).Returns(key);
            if (currencyCode != null)
            {
                var property = new Mock<IPublishedProperty>();
                property.Setup(p => p.GetValue(It.IsAny<string?>(), It.IsAny<string?>())).Returns(currencyCode);
                content.Setup(c => c.GetProperty(Consts.PropertyAlias.CurrencyCode)).Returns(property.Object);
            }

            return content.Object;
        }

        private void Parent(Guid child, Guid? parent)
            => _navigation.Setup(n => n.TryGetParentKey(child, out parent)).Returns(true);

        [Fact]
        public async Task GetCurrencyForContentAsync_UsesTheProductsOwnSiteRoot()
        {
            Guid siteB = Guid.NewGuid(), category = Guid.NewGuid(), product = Guid.NewGuid();
            Parent(product, category);
            Parent(category, siteB);
            Parent(siteB, null);
            _cache.Setup(c => c.GetByIdAsync(siteB, It.IsAny<bool?>())).ReturnsAsync(Content(siteB, "EUR"));

            Assert.Equal("EUR", await Service().GetCurrencyForContentAsync(Content(product)));
        }

        [Fact]
        public async Task GetCurrencyForContentAsync_NormalisesTheRootValue()
        {
            var root = Guid.NewGuid();
            Parent(root, null);

            Assert.Equal("GBP", await Service().GetCurrencyForContentAsync(Content(root, " gbp ")));
        }

        [Fact]
        public async Task GetCurrencyForContentAsync_FallsBackToConfiguration()
        {
            var root = Guid.NewGuid();
            Parent(root, null);
            _configuration.Setup(c => c.GetConfiguration()).ReturnsAsync(new UmbCheckoutConfiguration { CurrencyCode = "usd" });

            Assert.Equal("USD", await Service().GetCurrencyForContentAsync(Content(root)));
        }

        [Fact]
        public async Task GetCurrencyForContentAsync_IsNullWhenNothingIsConfigured()
        {
            var root = Guid.NewGuid();
            Parent(root, null);
            _configuration.Setup(c => c.GetConfiguration()).ReturnsAsync(new UmbCheckoutConfiguration());

            Assert.Null(await Service().GetCurrencyForContentAsync(Content(root)));
        }
    }
}
