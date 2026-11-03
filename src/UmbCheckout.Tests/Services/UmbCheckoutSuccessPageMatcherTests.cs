using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Services;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    /// <summary>The success page in any of its cultures and domains, and nothing else.</summary>
    public class UmbCheckoutSuccessPageMatcherTests
    {
        private static readonly Guid PageKey = Guid.Parse("38b08814-0307-4f92-930a-30751217a7c1");
        private readonly Mock<IPublishedContent> _page = new();

        private UmbCheckoutSuccessPageMatcher Matcher(string storedUrl = "/basket/", params (string? Culture, string Url)[] urls)
        {
            var configuration = new Mock<IConfigurationService>();
            configuration.Setup(c => c.GetConfiguration()).ReturnsAsync(new UmbCheckoutConfiguration
            {
                SuccessPageUrl = new[] { new MultiUrlPicker { Udi = $"umb://document/{PageKey:N}", Url = storedUrl } }
            });
            _page.Setup(p => p.Key).Returns(PageKey);
            _page.Setup(p => p.Cultures).Returns(urls.Where(u => u.Culture != null)
                .ToDictionary(u => u.Culture.Required(), u => new PublishedCultureInfo(u.Culture.Required(), "Basket", null, DateTime.UtcNow)));
            var cache = new Mock<IPublishedContentCache>();
            cache.Setup(c => c.GetById(PageKey)).Returns(urls.Length > 0 ? _page.Object : null);
            var umbracoContext = Mock.Of<IUmbracoContext>(c => c.Content == cache.Object);
            var accessor = new Mock<IUmbracoContextAccessor>();
            accessor.Setup(a => a.TryGetUmbracoContext(out umbracoContext)).Returns(true);
            var urlProvider = new Mock<IPublishedUrlProvider>();
            foreach (var (culture, url) in urls)
            {
                urlProvider.Setup(u => u.GetUrl(_page.Object, UrlMode.Absolute, culture, It.IsAny<Uri?>())).Returns(url);
            }

            return new UmbCheckoutSuccessPageMatcher(configuration.Object, accessor.Object, urlProvider.Object);
        }

        [Fact]
        public async Task The_routed_success_page_matches_on_any_culture_or_domain()
        {
            var matcher = Matcher(urls: ("en-GB", "https://shop.co.uk/basket/"));

            Assert.True(await matcher.IsSuccessPageAsync(_page.Object, new Uri("https://shop.fr/panier/?session_id=cs_test_1&success=true")));
            Assert.False(await matcher.IsSuccessPageAsync(Mock.Of<IPublishedContent>(p => p.Key == Guid.NewGuid()), new Uri("https://shop.co.uk/basket/?session_id=cs_test_1&success=true")));
        }

        [Fact]
        public async Task Every_culture_url_of_the_success_page_matches()
        {
            var matcher = Matcher(urls: new[] { ("en-GB", "https://shop.co.uk/basket/"), ((string?)"en-US", "https://shop.com/cart/") }.Select(u => ((string?)u.Item1, u.Item2)).ToArray());

            Assert.True(await matcher.IsSuccessPageAsync(new Uri("https://shop.co.uk/basket?session_id=cs_test_1&success=true")));
            Assert.True(await matcher.IsSuccessPageAsync(new Uri("https://SHOP.com/cart/?session_id=cs_test_1&success=true")));
            Assert.False(await matcher.IsSuccessPageAsync(new Uri("https://shop.com/basket/?session_id=cs_test_1&success=true")));
        }

        [Fact]
        public async Task An_invariant_page_uses_its_single_url()
        {
            var matcher = Matcher(urls: ((string?)null, "https://shop.test/basket/"));

            Assert.True(await matcher.IsSuccessPageAsync(new Uri("https://shop.test/basket/?session_id=cs_test_1")));
        }

        [Fact]
        public async Task A_page_not_in_the_cache_falls_back_to_the_stored_url()
        {
            var matcher = Matcher("/basket/");

            Assert.True(await matcher.IsSuccessPageAsync(new Uri("https://shop.test/Basket?session_id=cs_test_1")));
            Assert.False(await matcher.IsSuccessPageAsync(new Uri("https://shop.test/about/?session_id=cs_test_1")));
        }

        [Fact]
        public async Task No_success_page_configured_matches_nothing()
        {
            var configuration = new Mock<IConfigurationService>();
            configuration.Setup(c => c.GetConfiguration()).ReturnsAsync(new UmbCheckoutConfiguration());
            var matcher = new UmbCheckoutSuccessPageMatcher(configuration.Object, Mock.Of<IUmbracoContextAccessor>(), Mock.Of<IPublishedUrlProvider>());

            Assert.False(await matcher.IsSuccessPageAsync(_page.Object, new Uri("https://shop.test/basket/")));
        }
    }
}
