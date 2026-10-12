using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Services;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    /// <summary>The success page is matched by host and path in any of its cultures, so multilingual and multi-domain shops work.</summary>
    public class UmbCheckoutSuccessPageMatcherTests
    {
        private static readonly Guid PageKey = Guid.Parse("38b08814-0307-4f92-930a-30751217a7c1");

        private static UmbCheckoutSuccessPageMatcher Matcher(params (string Culture, string Url)[] urls)
        {
            var configuration = new Mock<IConfigurationService>();
            configuration.Setup(c => c.GetConfigurationAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new UmbCheckoutConfiguration
            {
                SuccessPageUrl = [new MultiUrlPicker { Udi = $"umb://document/{PageKey:N}", Url = "/basket/" }]
            });
            var page = new Mock<IPublishedContent>();
            page.Setup(p => p.Cultures).Returns(urls.ToDictionary(u => u.Culture, u => new PublishedCultureInfo(u.Culture, "Basket", null, DateTime.UtcNow)));
            var cache = new Mock<IPublishedContentCache>();
            cache.Setup(c => c.GetById(PageKey)).Returns(page.Object);
            var urlProvider = new Mock<IPublishedUrlProvider>();
            foreach (var (culture, url) in urls)
            {
                // Umbraco keys an invariant page's single culture as "".
                urlProvider.Setup(u => u.GetUrl(page.Object, UrlMode.Absolute, culture == "" ? null : culture, It.IsAny<Uri?>())).Returns(url);
            }

            return new UmbCheckoutSuccessPageMatcher(configuration.Object, cache.Object, urlProvider.Object);
        }

        [Fact]
        public async Task Clears_on_every_culture_url_of_the_success_page()
        {
            var matcher = Matcher(("en-GB", "https://shop.co.uk/basket/"), ("en-US", "https://shop.com/cart/"));

            Assert.True(await matcher.IsSuccessPageAsync(new Uri("https://shop.co.uk/basket?session_id=cs_test_1&success=true")));
            Assert.True(await matcher.IsSuccessPageAsync(new Uri("https://SHOP.com/cart/?session_id=cs_test_1&success=true")));
            Assert.False(await matcher.IsSuccessPageAsync(new Uri("https://shop.com/basket/?session_id=cs_test_1&success=true")));
        }

        [Fact]
        public async Task An_invariant_page_uses_its_single_url()
        {
            var matcher = Matcher(("", "https://shop.test/basket/"));

            Assert.True(await matcher.IsSuccessPageAsync(new Uri("https://shop.test/basket/?session_id=cs_test_1")));
            Assert.False(await matcher.IsSuccessPageAsync(new Uri("https://shop.test/about/?session_id=cs_test_1")));
        }

        [Fact]
        public async Task Without_a_configured_success_page_nothing_matches()
        {
            var configuration = new Mock<IConfigurationService>();
            configuration.Setup(c => c.GetConfigurationAsync(It.IsAny<CancellationToken>())).ReturnsAsync((UmbCheckoutConfiguration?)null);

            var matcher = new UmbCheckoutSuccessPageMatcher(configuration.Object, Mock.Of<IPublishedContentCache>(), Mock.Of<IPublishedUrlProvider>());

            Assert.False(await matcher.IsSuccessPageAsync(new Uri("https://shop.test/basket/")));
        }
    }
}
