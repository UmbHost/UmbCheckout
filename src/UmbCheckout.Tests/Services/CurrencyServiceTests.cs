using Microsoft.Extensions.Logging;
using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Services;
using UmbCheckout.Shared;
using UmbCheckout.Shared.Models;
using UmbCheckout.Tests.Helpers;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    /// <summary>The charged currency is decided on the server (the product's site root, then Configuration), never posted by the browser.</summary>
    public class CurrencyServiceTests : IDisposable
    {
        private readonly Mock<IConfigurationService> _configuration = new();
        private readonly RecordingLogger<CurrencyService> _logger = new();

        public CurrencyServiceTests() => CurrencyService.ResetNoCurrencyLog();

        public void Dispose() => CurrencyService.ResetNoCurrencyLog();

        private CurrencyService Service() => new(_configuration.Object, _logger, Mock.Of<IUmbracoContextAccessor>());

        private static IPublishedContent Content(IPublishedContent? parent, string? currencyCode = null)
        {
            var content = new Mock<IPublishedContent>();
            content.Setup(c => c.Key).Returns(Guid.NewGuid());
            content.Setup(c => c.Parent).Returns(parent);
            content.Setup(c => c.Level).Returns(parent == null ? 1 : parent.Level + 1);
            if (currencyCode != null)
            {
                var property = new Mock<IPublishedProperty>();
                property.Setup(p => p.GetValue(It.IsAny<string?>(), It.IsAny<string?>())).Returns(currencyCode);
                property.Setup(p => p.HasValue(It.IsAny<string?>(), It.IsAny<string?>())).Returns(!string.IsNullOrWhiteSpace(currencyCode));
                content.Setup(c => c.GetProperty(Consts.PropertyAlias.CurrencyCode)).Returns(property.Object);
            }

            return content.Object;
        }

        [Fact]
        public async Task GetCurrencyForContentAsync_UsesTheProductsOwnSiteRoot()
        {
            var siteA = Content(null, "GBP");
            var siteB = Content(null, "EUR");
            var product = Content(Content(siteB));

            Assert.Equal("EUR", await Service().GetCurrencyForContentAsync(product));
            Assert.Equal("GBP", await Service().GetCurrencyForContentAsync(Content(siteA)));
        }

        [Fact]
        public async Task GetCurrencyForContentAsync_NormalisesTheRootValue()
            => Assert.Equal("GBP", await Service().GetCurrencyForContentAsync(Content(Content(null, " gbp "))));

        [Fact]
        public async Task GetCurrencyForContentAsync_FallsBackToConfiguration()
        {
            _configuration.Setup(c => c.GetConfiguration()).ReturnsAsync(new UmbCheckoutConfiguration { CurrencyCode = "usd" });

            Assert.Equal("USD", await Service().GetCurrencyForContentAsync(Content(Content(null))));
        }

        [Fact]
        public async Task GetCurrencyForContentAsync_AnEmptyRootValueFallsBackToConfiguration()
        {
            _configuration.Setup(c => c.GetConfiguration()).ReturnsAsync(new UmbCheckoutConfiguration { CurrencyCode = "JPY" });

            Assert.Equal("JPY", await Service().GetCurrencyForContentAsync(Content(Content(null, ""))));
        }

        [Fact]
        public async Task GetCurrencyForContentAsync_IsNullWhenNothingIsConfigured_LoggedOncePerProcess()
        {
            _configuration.Setup(c => c.GetConfiguration()).ReturnsAsync(new UmbCheckoutConfiguration());

            // Called from every product render (the add-to-basket button): the error is logged once, not per render.
            Assert.Null(await Service().GetCurrencyForContentAsync(Content(Content(null))));
            Assert.Null(await Service().GetCurrencyForContentAsync(Content(Content(null))));

            Assert.Equal(1, _logger.Count(LogLevel.Error));
        }

        [Fact]
        public async Task GetCurrencyForContentAsync_NoConfigurationAtAllIsNull()
        {
            _configuration.Setup(c => c.GetConfiguration()).ReturnsAsync((UmbCheckoutConfiguration?)null);

            Assert.Null(await Service().GetCurrencyForContentAsync(Content(null)));
        }

        /// <summary>A third-party ICurrencyService written before GetCurrencyForContentAsync existed.</summary>
        private sealed class LegacyCurrencyService : ICurrencyService
        {
            private readonly string _code;

            public LegacyCurrencyService(string code) => _code = code;

            public Task<string> GetCurrencyAsync(Guid? currencyNodeKey = null) => Task.FromResult(_code);
        }

        [Theory]
        [InlineData("GBP", "GBP")]
        [InlineData(" eur ", "EUR")]
        [InlineData("", null)]
        [InlineData("Z", null)]
        [InlineData("G1P", null)]
        [InlineData("GBPX", null)]
        public async Task GetCurrencyForContentAsync_DefaultsToTheOldCurrencyWhenShapedLikeACode(string legacy, string? expected)
        {
            ICurrencyService service = new LegacyCurrencyService(legacy);

            Assert.Equal(expected, await service.GetCurrencyForContentAsync(Content(null)));
        }
    }
}
