using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.ViewComponents;
using UmbCheckout.Core.ViewModels;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.ViewComponents;
using UmbCheckout.Stripe.ViewModels;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Xunit;

namespace UmbCheckout.Tests.ViewComponents
{
    /// <summary>Money is displayed in the currency it is charged in.</summary>
    public class CurrencyDisplayViewComponentTests
    {
        private readonly Mock<IBasketService> _basketService = new();
        private readonly Mock<ICurrencyService> _currencyService = new();

        public CurrencyDisplayViewComponentTests()
        {
            // The old chain (currency node, current page's root, Configuration) answers USD.
            _currencyService.Setup(c => c.GetCurrencyAsync(It.IsAny<Guid?>())).ReturnsAsync("USD");
        }

        private void Basket(params string?[] currencies)
            => _basketService.Setup(b => b.Get()).ReturnsAsync(new Basket { LineItems = currencies.Select(c => new LineItem { Key = Guid.NewGuid(), Price = 1m, Quantity = 1, CurrencyCode = c }).ToList() });

        private static TModel Model<TModel>(IViewComponentResult result)
            => Assert.IsType<TModel>(Assert.IsType<ViewViewComponentResult>(result).ViewData!.Model);

        [Fact]
        public async Task StripeBasket_SubtotalIsInTheBasketsOwnCurrency()
        {
            Basket("GBP", "gbp");

            var result = await new StripeBasketViewComponent(_basketService.Object, _currencyService.Object).InvokeAsync();

            Assert.Equal("GBP", Model<StripeBasketViewModel>(result).FormatCurrency);
        }

        [Fact]
        public async Task StripeBasket_AnEmptyBasketUsesTheConfiguredCurrency()
        {
            Basket();

            var result = await new StripeBasketViewComponent(_basketService.Object, _currencyService.Object).InvokeAsync();

            Assert.Equal("USD", Model<StripeBasketViewModel>(result).FormatCurrency);
        }

        [Fact]
        public async Task BasketLink_SubtotalIsInTheBasketsOwnCurrency()
        {
            Basket("JPY");

            var result = await new BasketLinkViewComponent(_basketService.Object, Mock.Of<IUmbracoContextAccessor>(), _currencyService.Object).InvokeAsync();

            Assert.Equal("JPY", Model<BasketLinkViewModel>(result).CurrencyCode);
        }

        [Fact]
        public async Task BasketLink_ALegacyLineWithoutACurrencyUsesTheConfiguredCurrency()
        {
            Basket("");

            var result = await new BasketLinkViewComponent(_basketService.Object, Mock.Of<IUmbracoContextAccessor>(), _currencyService.Object).InvokeAsync();

            Assert.Equal("USD", Model<BasketLinkViewModel>(result).CurrencyCode);
        }

        [Fact]
        public async Task AddToBasketButton_ShowsTheChargedCurrency()
        {
            var product = Mock.Of<IPublishedContent>();
            _currencyService.Setup(c => c.GetCurrencyForContentAsync(product)).ReturnsAsync("JPY");

            // currencyNodeKey no longer decides the displayed currency: checkout charges the product's own.
            var result = await new StripeAddToBasketButtonViewComponent(_currencyService.Object).InvokeAsync(product, currencyNodeKey: Guid.NewGuid());

            Assert.Equal("JPY", Model<StripeAddToBasketButtonViewModel>(result).CurrencyCode);
        }
    }
}
