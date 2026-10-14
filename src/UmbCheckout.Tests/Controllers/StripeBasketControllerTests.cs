using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Models;
using UmbCheckout.Shared;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Controllers.Surface;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Tests.Helpers;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Web.Website.ActionResults;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Xunit;

namespace UmbCheckout.Tests.Controllers
{
    /// <summary>Add to basket: the price and currency come from the server; the posted currency is ignored.</summary>
    public class StripeBasketControllerTests
    {
        private readonly Mock<IBasketService> _basketService = new();
        private readonly Mock<IStripeLineItemPricingService> _pricing = new();
        private readonly Mock<IStripeBasketPricingService> _repricing = new();
        private readonly Mock<IStripeSessionService> _stripe = new();
        private readonly Mock<ISessionService> _sessions = new();
        private readonly RecordingLogger<StripeBasketController> _logger = new();
        private readonly IPublishedContent _product;
        private readonly List<LineItem> _basketLines = new();
        private readonly List<LineItem> _added = new();

        public StripeBasketControllerTests()
        {
            var product = new Mock<IPublishedContent>();
            product.Setup(p => p.Key).Returns(Guid.NewGuid());
            product.Setup(p => p.Name).Returns("Jacket");
            _product = product.Object;

            _basketService.Setup(b => b.Get(It.IsAny<CancellationToken>())).ReturnsAsync(() => new Basket { LineItems = _basketLines.ToList() });
            _basketService.Setup(b => b.Add(It.IsAny<LineItem>(), It.IsAny<CancellationToken>())).Callback<LineItem, CancellationToken>((line, _) => _added.Add(line)).ReturnsAsync(new Basket());
        }

        private StripeBasketController Controller(CancellationToken requestAborted = default)
        {
            var contentCache = new Mock<IPublishedContentCache>();
            contentCache.Setup(c => c.GetById(_product.Key)).Returns(_product);
            var umbracoContext = new Mock<IUmbracoContext>();
            umbracoContext.Setup(c => c.Content).Returns(contentCache.Object);
            var context = umbracoContext.Object;
            var accessor = new Mock<IUmbracoContextAccessor>();
            accessor.Setup(a => a.TryGetUmbracoContext(out context)).Returns(true);

            var page = new Mock<IPublishedContent>();
            page.Setup(p => p.Key).Returns(Guid.NewGuid());
            var request = new Mock<IPublishedRequest>();
            request.Setup(r => r.PublishedContent).Returns(page.Object);
            var httpContext = new DefaultHttpContext { RequestAborted = requestAborted };
            httpContext.Features.Set(new UmbracoRouteValues(request.Object, new ControllerActionDescriptor()));

            return new StripeBasketController(accessor.Object, Mock.Of<IUmbracoDatabaseFactory>(), ServiceContext.CreatePartial(), AppCaches.NoCache, Mock.Of<IProfilingLogger>(), Mock.Of<IPublishedUrlProvider>(), _basketService.Object, _stripe.Object, _pricing.Object, _repricing.Object, _sessions.Object, _logger)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            };
        }

#pragma warning disable CS0618 // BasketAdd.CurrencyCode is obsolete: these tests post it to prove it is ignored
        private BasketAdd Posted(string? currencyCode = "INR") => new() { Key = _product.Key, Quantity = 2, CurrencyCode = currencyCode };
#pragma warning restore CS0618

        [Fact]
        public async Task Add_UsesTheServersPriceAndCurrency_NotThePostedOne()
        {
            // £399 posted as "inr" used to be charged ₹399.
            _pricing.Setup(p => p.GetPriceAsync(_product, It.IsAny<CancellationToken>())).ReturnsAsync(new LineItemPrice(399m, "GBP"));
            var controller = Controller();

            await controller.Add(Posted("INR"));

            var line = Assert.Single(_added);
            Assert.Equal(_product.Key, line.Key);
            Assert.Equal(399m, line.Price);
            Assert.Equal("GBP", line.CurrencyCode);
            Assert.Equal(2, line.Quantity);
            Assert.Equal("Jacket", line.Name);
            Assert.Equal(_product.Key, controller.TempData[Consts.TempData.UmbCheckoutAddedToBasket]);
            Assert.False(controller.TempData.ContainsKey(Consts.TempData.UmbCheckoutAddToBasketFailed));
        }

        [Fact]
        public async Task Add_ARefusedPriceIsNotAdded()
        {
            // No price, or no currency configured: the pricing service has logged why.
            _pricing.Setup(p => p.GetPriceAsync(_product, It.IsAny<CancellationToken>())).ReturnsAsync((LineItemPrice?)null);
            var controller = Controller();

            var result = await controller.Add(Posted());

            Assert.Empty(_added);
            Assert.IsType<RedirectToUmbracoPageResult>(result);
            Assert.Equal(_product.Key, controller.TempData[Consts.TempData.UmbCheckoutAddToBasketFailed]);
            Assert.False(controller.TempData.ContainsKey(Consts.TempData.UmbCheckoutAddedToBasket));
            Assert.Equal(1, _logger.Count(LogLevel.Warning));
        }

        [Fact]
        public async Task Add_ALineTheBasketRefusesIsReported()
        {
            // A quantity outside 1-999,999, or a 101st product: refused by the basket, shown to the customer, never a 500.
            _pricing.Setup(p => p.GetPriceAsync(_product, It.IsAny<CancellationToken>())).ReturnsAsync(new LineItemPrice(10m, "GBP"));
            _basketService.Setup(b => b.Add(It.IsAny<LineItem>(), It.IsAny<CancellationToken>())).ThrowsAsync(new ArgumentOutOfRangeException("item"));
            var controller = Controller();

            var result = await controller.Add(new BasketAdd { Key = _product.Key, Quantity = 0 });

            Assert.IsType<RedirectToUmbracoPageResult>(result);
            Assert.Equal(_product.Key, controller.TempData[Consts.TempData.UmbCheckoutAddToBasketFailed]);
            Assert.False(controller.TempData.ContainsKey(Consts.TempData.UmbCheckoutAddedToBasket));
            Assert.Equal(1, _logger.Count(LogLevel.Warning));
            Assert.Equal(0, _logger.Count(LogLevel.Error));
        }

        [Fact]
        public async Task Add_ALineInAnotherCurrencyThanTheBasketsLinesIsAdded()
        {
            // Baskets are per currency: the basket service puts the line in its own currency's basket.
            _basketLines.Add(new LineItem { Key = Guid.NewGuid(), Price = 1m, CurrencyCode = "USD", Quantity = 1 });
            _pricing.Setup(p => p.GetPriceAsync(_product, It.IsAny<CancellationToken>())).ReturnsAsync(new LineItemPrice(10m, "GBP"));

            await Controller().Add(Posted());

            Assert.Equal("GBP", Assert.Single(_added).CurrencyCode);
        }

        [Fact]
        public async Task Reduce_IsAwaited()
        {
            var reduced = new TaskCompletionSource<Basket>();
            _basketService.Setup(b => b.Reduce(_product.Key, It.IsAny<CancellationToken>())).Returns(reduced.Task);

            var action = Controller().Reduce(_product.Key);

            Assert.False(action.IsCompleted);
            reduced.SetResult(new Basket());
            Assert.IsType<RedirectToUmbracoPageResult>(await action);
        }

        [Fact]
        public async Task Remove_IsAwaited()
        {
            var removed = new TaskCompletionSource<Basket>();
            _basketService.Setup(b => b.Remove(_product.Key, It.IsAny<CancellationToken>())).Returns(removed.Task);

            var action = Controller().Remove(_product.Key);

            Assert.False(action.IsCompleted);
            removed.SetResult(new Basket());
            Assert.IsType<RedirectToUmbracoPageResult>(await action);
        }

        [Fact]
        public async Task Reduce_AFailureIsLoggedWithTheProduct()
        {
            _basketService.Setup(b => b.Reduce(_product.Key, It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("boom"));

            var result = await Controller().Reduce(_product.Key);

            Assert.Equal(500, Assert.IsType<StatusCodeResult>(result).StatusCode);
            Assert.Equal(1, _logger.Count(LogLevel.Error));
        }

        [Fact]
        public async Task Checkout_AnUnchangedBasketGoesToStripe()
        {
            var basket = new Basket { CurrencyCode = "GBP", LineItems = new List<LineItem> { new() { Key = _product.Key, Price = 10m, CurrencyCode = "GBP", Quantity = 1 } } };
            _basketService.Setup(b => b.Get(It.IsAny<CancellationToken>())).ReturnsAsync(basket);
            _repricing.Setup(r => r.RepriceAsync(basket, It.IsAny<CancellationToken>())).ReturnsAsync(new BasketRepricing(basket, Array.Empty<RepricedLine>()));
            _stripe.Setup(s => s.CreateSessionAsync(basket, It.IsAny<CancellationToken>())).ReturnsAsync(new global::Stripe.Checkout.Session { Id = "cs_test_1", Url = "https://checkout.stripe.com/c/pay/cs_test_1" });

            var result = await Controller().Checkout();

            Assert.Equal("https://checkout.stripe.com/c/pay/cs_test_1", Assert.IsType<RedirectResult>(result).Url);
        }

        [Fact]
        public async Task Checkout_records_the_Stripe_session_on_the_basket_it_pays_for()
        {
            // The success page (and the webhook) clear this basket only when it returns from this checkout session.
            var basket = new Basket { CurrencyCode = "GBP", LineItems = new List<LineItem> { new() { Key = _product.Key, Price = 10m, CurrencyCode = "GBP", Quantity = 1 } } };
            _basketService.Setup(b => b.Get(It.IsAny<CancellationToken>())).ReturnsAsync(basket);
            _repricing.Setup(r => r.RepriceAsync(basket, It.IsAny<CancellationToken>())).ReturnsAsync(new BasketRepricing(basket, Array.Empty<RepricedLine>()));
            _stripe.Setup(s => s.CreateSessionAsync(basket, It.IsAny<CancellationToken>())).ReturnsAsync(new global::Stripe.Checkout.Session { Id = "cs_test_1", Url = "https://checkout.stripe.com/c/pay/cs_test_1" });

            await Controller().Checkout();

            _sessions.Verify(s => s.Update(It.Is<Basket>(b => b.CheckoutSessionId == "cs_test_1"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Checkout_ChangedPricesReturnToTheBasketWithANotice()
        {
            // The customer sees the current prices before paying; no Stripe session is created on this click.
            var basket = new Basket { CurrencyCode = "GBP", LineItems = new List<LineItem> { new() { Key = _product.Key, Name = "Jacket", Price = 10m, CurrencyCode = "GBP", Quantity = 1 } } };
            _basketService.Setup(b => b.Get(It.IsAny<CancellationToken>())).ReturnsAsync(basket);
            _repricing.Setup(r => r.RepriceAsync(basket, It.IsAny<CancellationToken>())).ReturnsAsync(new BasketRepricing(basket, new[] { new RepricedLine(_product.Key, "Jacket", 10m, 12m), new RepricedLine(Guid.NewGuid(), "Hat", 5m, null) }));
            var controller = Controller();

            var result = await controller.Checkout();

            Assert.IsType<RedirectToUmbracoPageResult>(result);
            Assert.Equal("Jacket, Hat", controller.TempData[Consts.TempData.UmbCheckoutBasketRepriced]);
            _sessions.Verify(s => s.Update(basket, It.IsAny<CancellationToken>()), Times.Once);
            _stripe.Verify(s => s.CreateSessionAsync(It.IsAny<Basket>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Checkout_AnEmptyBasketIsNotSentToStripe()
        {
            _basketService.Setup(b => b.Get(It.IsAny<CancellationToken>())).ReturnsAsync(new Basket());
            var controller = Controller();

            await controller.Checkout();

            Assert.Equal(true, controller.TempData[Consts.TempData.UmbCheckoutEmptyBasket]);
            _repricing.Verify(r => r.RepriceAsync(It.IsAny<Basket>(), It.IsAny<CancellationToken>()), Times.Never);
            _stripe.Verify(s => s.CreateSessionAsync(It.IsAny<Basket>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Checkout_passes_the_requests_token_to_pricing_and_Stripe_and_records_the_session_regardless()
        {
            using var request = new CancellationTokenSource();
            var basket = new Basket { CurrencyCode = "GBP", LineItems = new List<LineItem> { new() { Key = _product.Key, Price = 10m, CurrencyCode = "GBP", Quantity = 1 } } };
            _basketService.Setup(b => b.Get(It.IsAny<CancellationToken>())).ReturnsAsync(basket);
            _repricing.Setup(r => r.RepriceAsync(basket, It.IsAny<CancellationToken>())).ReturnsAsync(new BasketRepricing(basket, Array.Empty<RepricedLine>()));
            _stripe.Setup(s => s.CreateSessionAsync(basket, It.IsAny<CancellationToken>())).ReturnsAsync(new global::Stripe.Checkout.Session { Id = "cs_test_1", Url = "https://checkout.stripe.com/c/pay/cs_test_1" });

            await Controller(request.Token).Checkout();

            _basketService.Verify(b => b.Get(request.Token), Times.Once);
            _repricing.Verify(r => r.RepriceAsync(basket, request.Token), Times.Once);
            _stripe.Verify(s => s.CreateSessionAsync(basket, request.Token), Times.Once);
            // Once Stripe has the session, the basket records it even if the customer has left.
            _sessions.Verify(s => s.Update(It.Is<Basket>(b => b.CheckoutSessionId == "cs_test_1"), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Checkout_a_cancelled_request_returns_nothing_and_logs_no_error()
        {
            using var request = new CancellationTokenSource();
            var basket = new Basket { CurrencyCode = "GBP", LineItems = new List<LineItem> { new() { Key = _product.Key, Price = 10m, CurrencyCode = "GBP", Quantity = 1 } } };
            _basketService.Setup(b => b.Get(It.IsAny<CancellationToken>())).ReturnsAsync(basket);
            _repricing.Setup(r => r.RepriceAsync(basket, It.IsAny<CancellationToken>())).ReturnsAsync(new BasketRepricing(basket, Array.Empty<RepricedLine>()));
            _stripe.Setup(s => s.CreateSessionAsync(basket, It.IsAny<CancellationToken>())).Callback(request.Cancel).ThrowsAsync(new OperationCanceledException(request.Token));

            var result = await Controller(request.Token).Checkout();

            Assert.IsType<EmptyResult>(result);
            Assert.Equal(0, _logger.Count(LogLevel.Error));
            _sessions.Verify(s => s.Update(It.IsAny<Basket>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Add_Reduce_Remove_and_EmptyBasket_pass_the_requests_token()
        {
            using var request = new CancellationTokenSource();
            _pricing.Setup(p => p.GetPriceAsync(_product, It.IsAny<CancellationToken>())).ReturnsAsync(new LineItemPrice(10m, "GBP"));

            await Controller(request.Token).Add(Posted());
            await Controller(request.Token).Reduce(_product.Key);
            await Controller(request.Token).Remove(_product.Key);
            await Controller(request.Token).EmptyBasket();

            _pricing.Verify(p => p.GetPriceAsync(_product, request.Token), Times.Once);
            _basketService.Verify(b => b.Add(It.IsAny<LineItem>(), request.Token), Times.Once);
            _basketService.Verify(b => b.Reduce(_product.Key, request.Token), Times.Once);
            _basketService.Verify(b => b.Remove(_product.Key, request.Token), Times.Once);
            _basketService.Verify(b => b.Clear(request.Token), Times.Once);
        }

        [Fact]
        public void TheOlderConstructorsAreObsolete()
            => Assert.Equal(2, typeof(StripeBasketController).GetConstructors().Count(c => c.GetCustomAttributes(typeof(ObsoleteAttribute), false).Any()));

        [Fact]
        public void BasketAdd_CurrencyCodeIsObsolete()
            => Assert.NotNull(typeof(BasketAdd).GetProperty("CurrencyCode").Required().GetCustomAttributes(typeof(ObsoleteAttribute), false).SingleOrDefault());

        [Fact]
        public void DependencyInjection_UsesTheConstructorWithTheRepricing()
        {
            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<IUmbracoContextAccessor>());
            services.AddSingleton(Mock.Of<IUmbracoDatabaseFactory>());
            services.AddSingleton(ServiceContext.CreatePartial());
            services.AddSingleton(AppCaches.NoCache);
            services.AddSingleton(Mock.Of<IProfilingLogger>());
            services.AddSingleton(Mock.Of<IPublishedUrlProvider>());
            services.AddSingleton(_basketService.Object);
            services.AddSingleton(Mock.Of<IStripeSessionService>());
            services.AddSingleton(_pricing.Object);
            services.AddSingleton(_repricing.Object);
            services.AddSingleton(_sessions.Object);
            services.AddSingleton<ILogger<StripeBasketController>>(_logger);

            // MVC activates controllers through ActivatorUtilities; three public constructors must not be ambiguous.
            var factory = ActivatorUtilities.CreateFactory(typeof(StripeBasketController), Type.EmptyTypes);

            Assert.IsType<StripeBasketController>(factory(services.BuildServiceProvider(), null));
        }
    }
}
