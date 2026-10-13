using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Services;
using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    /// <summary>The current request's basket: the page's site root culture and the currency the page sells in.</summary>
    public class UmbracoBasketContextTests
    {
        private readonly Mock<ICurrencyService> _currency = new();
        private readonly Mock<IConfigurationService> _configuration = new();
        private readonly Mock<IUmbracoContextAccessor> _accessor = new();

        private UmbracoBasketContext Context() => new(_accessor.Object, _currency.Object, _configuration.Object);

        private void OnPage(IPublishedContent? page)
        {
            var request = new Mock<IPublishedRequest>();
            request.Setup(r => r.PublishedContent).Returns(page);
            var umbracoContext = new Mock<IUmbracoContext>();
            umbracoContext.Setup(c => c.PublishedRequest).Returns(request.Object);
            var context = umbracoContext.Object;
            _accessor.Setup(a => a.TryGetUmbracoContext(out context)).Returns(true);
        }

        [Fact]
        public async Task GetCurrencyAsync_IsTheCurrentPagesCurrency()
        {
            var page = Mock.Of<IPublishedContent>();
            OnPage(page);
            _currency.Setup(c => c.GetCurrencyForContentAsync(page, It.IsAny<CancellationToken>())).ReturnsAsync("EUR");

            Assert.Equal("EUR", await Context().GetCurrencyAsync());
        }

        [Fact]
        public async Task GetCurrencyAsync_WithoutAPageIsTheConfigurationCurrency()
        {
            OnPage(null);
            _configuration.Setup(c => c.GetConfiguration()).ReturnsAsync(new UmbCheckoutConfiguration { CurrencyCode = " usd " });

            Assert.Equal("USD", await Context().GetCurrencyAsync());
        }

        [Fact]
        public async Task GetCurrencyAsync_WithoutAnUmbracoContextIsTheConfigurationCurrency()
        {
            _configuration.Setup(c => c.GetConfiguration()).ReturnsAsync(new UmbCheckoutConfiguration { CurrencyCode = "GBP" });

            Assert.Equal("GBP", await Context().GetCurrencyAsync());
        }

        [Fact]
        public async Task GetCurrencyAsync_IsNullWhenNothingIsConfigured()
            => Assert.Null(await Context().GetCurrencyAsync());

        [Fact]
        public void SiteRootCulture_IsNullWithoutAPage()
            => Assert.Null(Context().SiteRootCulture);
    }
}
