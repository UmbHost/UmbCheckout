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

            _basketService.Setup(b => b.Get()).ReturnsAsync(() => new Basket { LineItems = _basketLines.ToList() });
            _basketService.Setup(b => b.Add(It.IsAny<LineItem>())).Callback<LineItem>(_added.Add).ReturnsAsync(new Basket());
        }

        private StripeBasketController Controller()
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
            var httpContext = new DefaultHttpContext();
            httpContext.Features.Set(new UmbracoRouteValues(request.Object, new ControllerActionDescriptor()));

            return new StripeBasketController(accessor.Object, Mock.Of<IUmbracoDatabaseFactory>(), ServiceContext.CreatePartial(), AppCaches.NoCache, Mock.Of<IProfilingLogger>(), Mock.Of<IPublishedUrlProvider>(), _basketService.Object, Mock.Of<IStripeSessionService>(), _pricing.Object, _logger)
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
            _pricing.Setup(p => p.GetPriceAsync(_product)).ReturnsAsync(new LineItemPrice(399m, "GBP"));
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
            _pricing.Setup(p => p.GetPriceAsync(_product)).ReturnsAsync((LineItemPrice?)null);
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
            _pricing.Setup(p => p.GetPriceAsync(_product)).ReturnsAsync(new LineItemPrice(10m, "GBP"));
            _basketService.Setup(b => b.Add(It.IsAny<LineItem>())).ThrowsAsync(new ArgumentOutOfRangeException("item"));
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
            _pricing.Setup(p => p.GetPriceAsync(_product)).ReturnsAsync(new LineItemPrice(10m, "GBP"));

            await Controller().Add(Posted());

            Assert.Equal("GBP", Assert.Single(_added).CurrencyCode);
        }

        [Fact]
        public async Task Reduce_IsAwaited()
        {
            var reduced = new TaskCompletionSource<Basket>();
            _basketService.Setup(b => b.Reduce(_product.Key)).Returns(reduced.Task);

            var action = Controller().Reduce(_product.Key);

            Assert.False(action.IsCompleted);
            reduced.SetResult(new Basket());
            Assert.IsType<RedirectToUmbracoPageResult>(await action);
        }

        [Fact]
        public async Task Remove_IsAwaited()
        {
            var removed = new TaskCompletionSource<Basket>();
            _basketService.Setup(b => b.Remove(_product.Key)).Returns(removed.Task);

            var action = Controller().Remove(_product.Key);

            Assert.False(action.IsCompleted);
            removed.SetResult(new Basket());
            Assert.IsType<RedirectToUmbracoPageResult>(await action);
        }

        [Fact]
        public async Task Reduce_AFailureIsLoggedWithTheProduct()
        {
            _basketService.Setup(b => b.Reduce(_product.Key)).ThrowsAsync(new InvalidOperationException("boom"));

            var result = await Controller().Reduce(_product.Key);

            Assert.Equal(500, Assert.IsType<StatusCodeResult>(result).StatusCode);
            Assert.Equal(1, _logger.Count(LogLevel.Error));
        }

        [Fact]
        public void BasketAdd_CurrencyCodeIsObsolete()
            => Assert.NotNull(typeof(BasketAdd).GetProperty("CurrencyCode")!.GetCustomAttributes(typeof(ObsoleteAttribute), false).SingleOrDefault());

        [Fact]
        public void DependencyInjection_UsesTheConstructorWithThePricingService()
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
            services.AddSingleton<ILogger<StripeBasketController>>(_logger);

            // MVC activates controllers through ActivatorUtilities; two public constructors must not be ambiguous.
            var factory = ActivatorUtilities.CreateFactory(typeof(StripeBasketController), Type.EmptyTypes);

            Assert.IsType<StripeBasketController>(factory(services.BuildServiceProvider(), null));
        }
    }
}
