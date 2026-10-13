using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.ViewComponents;
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
            var result = await Button().InvokeAsync(product, currencyNodeKey: Guid.NewGuid());

            Assert.Equal("JPY", Model<StripeAddToBasketButtonViewModel>(result).CurrencyCode);
        }

        [Fact]
        public async Task AddToBasketButton_ANullProductRendersNothingWithoutAskingForACurrency()
        {
            // The partial renders nothing for a null product, as before; the currency service is never handed null.
            IViewComponentResult? result = null;
            var exception = await Record.ExceptionAsync(async () => result = await Button().InvokeAsync(null!));

            Assert.Null(exception);
            var model = Model<StripeAddToBasketButtonViewModel>(result!);
            Assert.Null(model.Product);
            Assert.Null(model.CurrencyCode);
            _currencyService.Verify(c => c.GetCurrencyForContentAsync(It.IsAny<IPublishedContent>()), Times.Never);
        }

        [Fact]
        public async Task AddToBasketButton_ACurrencyNodeKeyInAnotherCurrencyIsWarnedOncePerProcess()
        {
            var product = Mock.Of<IPublishedContent>();
            var nodeKey = Guid.NewGuid();
            _currencyService.Setup(c => c.GetCurrencyForContentAsync(product)).ReturnsAsync("GBP");
            _currencyService.Setup(c => c.GetCurrencyAsync(nodeKey)).ReturnsAsync("EUR");

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
            _currencyService.Setup(c => c.GetCurrencyForContentAsync(product)).ReturnsAsync("GBP");
            _currencyService.Setup(c => c.GetCurrencyAsync(nodeKey)).ReturnsAsync(nodeCurrency);

            await Button().InvokeAsync(product, currencyNodeKey: nodeKey);

            Assert.Empty(_buttonLogger.Entries);
        }

        [Fact]
        public async Task AddToBasketButton_WithoutACurrencyNodeKeyTheOldChainIsNotAsked()
        {
            var product = Mock.Of<IPublishedContent>();
            _currencyService.Setup(c => c.GetCurrencyForContentAsync(product)).ReturnsAsync("GBP");

            await Button().InvokeAsync(product);

            _currencyService.Verify(c => c.GetCurrencyAsync(It.IsAny<Guid?>()), Times.Never);
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
