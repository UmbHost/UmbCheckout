using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.ViewComponents;
using UmbCheckout.Core.ViewModels;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.ViewComponents;
using UmbCheckout.Stripe.ViewModels;
using UmbCheckout.Tests.Helpers;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Xunit;

namespace UmbCheckout.Tests.ViewComponents
{
    /// <summary>Money is displayed in the currency it is charged in.</summary>
    public class CurrencyDisplayViewComponentTests : IDisposable
    {
        private readonly Mock<IBasketService> _basketService = new();
        private readonly Mock<ICurrencyService> _currencyService = new();
        private readonly RecordingLogger<StripeAddToBasketButtonViewComponent> _buttonLogger = new();

        public void Dispose() => StripeAddToBasketButtonViewComponent.ResetCurrencyNodeKeyWarning();

        private StripeAddToBasketButtonViewComponent Button() => new(_currencyService.Object, _buttonLogger);

        public CurrencyDisplayViewComponentTests()
        {
            StripeAddToBasketButtonViewComponent.ResetCurrencyNodeKeyWarning();

            // The old chain (currency node, current page's root, Configuration) answers USD.
            _currencyService.Setup(c => c.GetCurrencyAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>())).ReturnsAsync("USD");
        }

        private void Basket(params string?[] currencies)
            => _basketService.Setup(b => b.Get(It.IsAny<CancellationToken>())).ReturnsAsync(new Basket { LineItems = currencies.Select(c => new LineItem { Key = Guid.NewGuid(), Price = 1m, Quantity = 1, CurrencyCode = c }).ToList() });

        private static TModel Model<TModel>(IViewComponentResult result)
            => Assert.IsType<TModel>(Assert.IsType<ViewViewComponentResult>(result).ViewData.Required().Model);

        [Fact]
        public async Task StripeBasket_SubtotalIsInTheBasketsOwnCurrency()
        {
            Basket("GBP", "gbp");

            var result = await new StripeBasketViewComponent(_basketService.Object, _currencyService.Object).InvokeAsync();

            Assert.Equal("GBP", Model<StripeBasketViewModel>(result).FormatCurrency);
        }

        [Fact]
        public async Task StripeBasket_TheBasketsOwnCurrencyComesFirst()
        {
            _basketService.Setup(b => b.Get(It.IsAny<CancellationToken>())).ReturnsAsync(new Basket { CurrencyCode = "EUR" });

            var result = await new StripeBasketViewComponent(_basketService.Object, _currencyService.Object).InvokeAsync();

            Assert.Equal("EUR", Model<StripeBasketViewModel>(result).FormatCurrency);
        }

        [Fact]
        public async Task StripeBasket_ShowsARefusedAddAndChangedPrices()
        {
            Basket("GBP");
            var component = WithTempData(new StripeBasketViewComponent(_basketService.Object, _currencyService.Object), out var tempData);
            tempData[Shared.Consts.TempData.UmbCheckoutAddToBasketFailed] = Guid.NewGuid();
            tempData[Shared.Consts.TempData.UmbCheckoutBasketRepriced] = "Jacket, Hat";

            var notices = Model<StripeBasketViewModel>(await component.InvokeAsync(addToBasketFailedText: "Not added.", pricesChangedText: "Prices changed:")).Notices;

            Assert.Equal(new[] { "Not added.", "Prices changed: Jacket, Hat" }, notices);
        }

        [Fact]
        public async Task StripeBasket_NoNoticesByDefault()
        {
            Basket("GBP");
            var component = WithTempData(new StripeBasketViewComponent(_basketService.Object, _currencyService.Object), out _);

            Assert.Empty(Model<StripeBasketViewModel>(await component.InvokeAsync()).Notices);
        }

        [Fact]
        public async Task BasketLink_TheBasketsOwnCurrencyComesFirst()
        {
            _basketService.Setup(b => b.Get(It.IsAny<CancellationToken>())).ReturnsAsync(new Basket { CurrencyCode = "EUR" });

            var result = await new BasketLinkViewComponent(_basketService.Object, Mock.Of<IUmbracoContextAccessor>(), _currencyService.Object).InvokeAsync();

            Assert.Equal("EUR", Model<BasketLinkViewModel>(result).CurrencyCode);
        }

        private static T WithTempData<T>(T component, out ITempDataDictionary tempData) where T : ViewComponent
        {
            tempData = new TempDataDictionary(new Microsoft.AspNetCore.Http.DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            component.ViewComponentContext = new ViewComponentContext { ViewContext = new ViewContext { TempData = tempData } };
            return component;
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
            _currencyService.Setup(c => c.GetCurrencyForContentAsync(product, It.IsAny<CancellationToken>())).ReturnsAsync("JPY");

            // currencyNodeKey no longer decides the displayed currency: checkout charges the product's own.
            var result = await Button().InvokeAsync(product, currencyNodeKey: Guid.NewGuid());

            Assert.Equal("JPY", Model<StripeAddToBasketButtonViewModel>(result).CurrencyCode);
        }

        [Fact]
        public async Task AddToBasketButton_ANullProductRendersNothingWithoutAskingForACurrency()
        {
            // The partial renders nothing for a null product, as before; the currency service is never handed null.
            IViewComponentResult? result = null;
            var exception = await Record.ExceptionAsync(async () => result = await Button().InvokeAsync(null));

            Assert.Null(exception);
            var model = Model<StripeAddToBasketButtonViewModel>(result.Required());
            Assert.Null(model.Product);
            Assert.Null(model.CurrencyCode);
            _currencyService.Verify(c => c.GetCurrencyForContentAsync(It.IsAny<IPublishedContent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AddToBasketButton_ACurrencyNodeKeyInAnotherCurrencyIsWarnedOncePerProcess()
        {
            var product = Mock.Of<IPublishedContent>();
            var nodeKey = Guid.NewGuid();
            _currencyService.Setup(c => c.GetCurrencyForContentAsync(product, It.IsAny<CancellationToken>())).ReturnsAsync("GBP");
            _currencyService.Setup(c => c.GetCurrencyAsync(nodeKey, It.IsAny<CancellationToken>())).ReturnsAsync("EUR");

            // An upgraded site that used currencyNodeKey to show EUR is now charged GBP (the site root, then Configuration).
            await Button().InvokeAsync(product, currencyNodeKey: nodeKey);
            await Button().InvokeAsync(product, currencyNodeKey: nodeKey);

            var warning = Assert.Single(_buttonLogger.Entries);
            Assert.Equal(LogLevel.Warning, warning.Level);
            Assert.Contains("EUR", warning.Message);
            Assert.Contains("GBP", warning.Message);
            Assert.DoesNotContain(nodeKey.ToString(), warning.Message);
        }

        [Theory]
        [InlineData("GBP")]
        [InlineData("gbp")]
        public async Task AddToBasketButton_ACurrencyNodeKeyInTheChargedCurrencyIsNotWarned(string nodeCurrency)
        {
            var product = Mock.Of<IPublishedContent>();
            var nodeKey = Guid.NewGuid();
            _currencyService.Setup(c => c.GetCurrencyForContentAsync(product, It.IsAny<CancellationToken>())).ReturnsAsync("GBP");
            _currencyService.Setup(c => c.GetCurrencyAsync(nodeKey, It.IsAny<CancellationToken>())).ReturnsAsync(nodeCurrency);

            await Button().InvokeAsync(product, currencyNodeKey: nodeKey);

            Assert.Empty(_buttonLogger.Entries);
        }

        [Fact]
        public async Task AddToBasketButton_WithoutACurrencyNodeKeyTheOldChainIsNotAsked()
        {
            var product = Mock.Of<IPublishedContent>();
            _currencyService.Setup(c => c.GetCurrencyForContentAsync(product, It.IsAny<CancellationToken>())).ReturnsAsync("GBP");

            await Button().InvokeAsync(product);

            _currencyService.Verify(c => c.GetCurrencyAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
            Assert.Empty(_buttonLogger.Entries);
        }

        [Fact]
        public void AddToBasketButton_TheActivatorChoosesTheLoggingConstructor()
        {
            var services = new ServiceCollection();
            services.AddSingleton(_currencyService.Object);
            services.AddSingleton<ILogger<StripeAddToBasketButtonViewComponent>>(_buttonLogger);

            // View components are created through ActivatorUtilities; two public constructors must not be ambiguous.
            var factory = ActivatorUtilities.CreateFactory(typeof(StripeAddToBasketButtonViewComponent), Type.EmptyTypes);

            Assert.IsType<StripeAddToBasketButtonViewComponent>(factory(services.BuildServiceProvider(), null));
        }
    }
}
