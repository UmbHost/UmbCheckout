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
    public class UmbracoBasketContextTests
    {
        private readonly Mock<ICurrencyService> _currency = new();
        private readonly Mock<IConfigurationService> _configuration = new();
        private readonly Mock<IUmbracoContextAccessor> _accessor = new();

        private UmbracoBasketContext Context() => new(_accessor.Object, _currency.Object, _configuration.Object);

        [Fact]
        public async Task GetCurrencyAsync_IsTheCurrentPagesCurrency()
        {
            var page = Mock.Of<IPublishedContent>();
            var request = new Mock<IPublishedRequest>();
            request.Setup(r => r.PublishedContent).Returns(page);
            var umbracoContext = new Mock<IUmbracoContext>();
            umbracoContext.Setup(c => c.PublishedRequest).Returns(request.Object);
            var context = umbracoContext.Object;
            _accessor.Setup(a => a.TryGetUmbracoContext(out context)).Returns(true);
            _currency.Setup(c => c.GetCurrencyForContentAsync(page, It.IsAny<CancellationToken>())).ReturnsAsync("EUR");

            Assert.Equal("EUR", await Context().GetCurrencyAsync());
        }

        [Fact]
        public async Task GetCurrencyAsync_WithoutAPageIsTheConfigurationCurrency()
        {
            _configuration.Setup(c => c.GetConfigurationAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new UmbCheckoutConfiguration { CurrencyCode = " usd " });

            Assert.Equal("USD", await Context().GetCurrencyAsync());
        }

        [Fact]
        public async Task GetCurrencyAsync_IsNullWhenNothingIsConfigured()
            => Assert.Null(await Context().GetCurrencyAsync());
    }
}
