using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Models;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Controllers.Surface;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Shared.Notifications;
using UmbCheckout.Testing;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Routing;
using Xunit;
using Consts = UmbCheckout.Shared.Consts;

namespace UmbCheckout.Tests.Services
{
    /// <summary>A basket change a site's handler cancelled: the shopper sees the handler's reason, not a confirmation.</summary>
    public class StripeBasketControllerCancelTests
    {
        private readonly Mock<IBasketService> _basket = new();
        private readonly Mock<IStripeBasketPricingService> _repricing = new();
        private readonly Mock<IStripeSessionService> _stripe = new();
        private readonly Mock<ISessionService> _sessions = new();
        private readonly Mock<IPublishedContent> _product = new();
        private readonly TestEventMessagesFactory _messages = new();

        public StripeBasketControllerCancelTests()
        {
            _product.SetupGet(p => p.Key).Returns(Guid.NewGuid());
            _product.SetupGet(p => p.Name).Returns("Ball");
        }

        private StripeBasketController Controller(string? cancelMessage)
        {
            var umbracoContext = new Mock<IUmbracoContext>();
            umbracoContext.SetupGet(c => c.Content).Returns(Mock.Of<IPublishedContentCache>(c => c.GetById(_product.Object.Key) == _product.Object));
            var context = umbracoContext.Object;
            var accessor = new Mock<IUmbracoContextAccessor>();
            accessor.Setup(a => a.TryGetUmbracoContext(out context)).Returns(true);
            var pricing = new Mock<IStripeLineItemPricingService>();
            pricing.Setup(p => p.GetPriceAsync(_product.Object, It.IsAny<CancellationToken>())).ReturnsAsync(new LineItemPrice(2.5m, "GBP"));

            var controller = new StripeBasketController(accessor.Object, Mock.Of<IUmbracoDatabaseFactory>(), ServiceContext.CreatePartial(), AppCaches.NoCache,
                Mock.Of<IProfilingLogger>(), Mock.Of<IPublishedUrlProvider>(), _basket.Object, _stripe.Object, pricing.Object, _repricing.Object, _sessions.Object,
                NullLogger<StripeBasketController>.Instance);

            if (cancelMessage != null)
            {
                _messages.Messages.Add(new EventMessage("Basket", cancelMessage, EventMessageType.Error));
                _messages.Messages.MarkCancelled();
            }

            var http = new DefaultHttpContext { RequestServices = new ServiceCollection().AddSingleton<IEventMessagesFactory>(_messages).BuildServiceProvider() };
            var page = Mock.Of<IPublishedRequest>(r => r.PublishedContent == _product.Object);
            http.Features.Set(new UmbracoRouteValues(page, new ControllerActionDescriptor()));
            controller.ControllerContext = new ControllerContext { HttpContext = http };
            controller.TempData = new TempDataDictionary(http, Mock.Of<ITempDataProvider>());
            return controller;
        }

        [Fact]
        public async Task A_cancelled_add_keeps_the_basket_and_says_why()
        {
            var controller = Controller("Only one ball per customer");

            await controller.Add(new BasketAdd { Key = _product.Object.Key, Quantity = 1 }, redirectGuid: Guid.NewGuid());

            Assert.Equal("Only one ball per customer", controller.TempData[Consts.TempData.UmbCheckoutBasketChangeCancelled]);
            Assert.Null(controller.TempData[Consts.TempData.UmbCheckoutAddedToBasket]);
        }

        [Fact]
        public async Task An_add_without_a_cancel_still_confirms()
        {
            var controller = Controller(null);

            await controller.Add(new BasketAdd { Key = _product.Object.Key, Quantity = 1 }, redirectGuid: Guid.NewGuid());

            Assert.Null(controller.TempData[Consts.TempData.UmbCheckoutBasketChangeCancelled]);
            Assert.Equal(_product.Object.Key, controller.TempData[Consts.TempData.UmbCheckoutAddedToBasket]);
        }

        [Fact]
        public async Task A_cancelled_reduce_or_remove_says_why()
        {
            var reduce = Controller("Locked");
            await reduce.Reduce(Guid.NewGuid(), redirectGuid: Guid.NewGuid());
            var remove = Controller("Locked");
            await remove.Remove(Guid.NewGuid(), redirectGuid: Guid.NewGuid());

            Assert.Equal("Locked", reduce.TempData[Consts.TempData.UmbCheckoutBasketChangeCancelled]);
            Assert.Null(reduce.TempData[Consts.TempData.UmbCheckoutBasketReduced]);
            Assert.Equal("Locked", remove.TempData[Consts.TempData.UmbCheckoutBasketChangeCancelled]);
            Assert.Null(remove.TempData[Consts.TempData.UmbCheckoutBasketRemoved]);
        }

        [Fact]
        public async Task A_cancelled_reprice_update_says_why_and_does_not_reach_Stripe()
        {
            var basket = new Basket { CurrencyCode = "GBP", LineItems = [new LineItem { Key = Guid.NewGuid(), Name = "Ball", Price = 1m, Quantity = 1, CurrencyCode = "GBP" }] };
            _basket.Setup(b => b.Get(It.IsAny<CancellationToken>())).ReturnsAsync(basket);
            _repricing.Setup(p => p.RepriceAsync(basket, It.IsAny<CancellationToken>())).ReturnsAsync(new BasketRepricing(basket, [new RepricedLine(basket.LineItems.Single().Key, "Ball", 1m, 2.5m)]));
            var controller = Controller("Prices are frozen");

            await controller.Checkout();

            Assert.Equal("Prices are frozen", controller.TempData[Consts.TempData.UmbCheckoutBasketChangeCancelled]);
            Assert.Null(controller.TempData[Consts.TempData.UmbCheckoutBasketRepriced]);
            _stripe.Verify(s => s.CreateSessionAsync(It.IsAny<Basket>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
