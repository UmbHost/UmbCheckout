using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Session = Stripe.Checkout.Session;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Services;
using UmbCheckout.Shared.Models;
using UmbCheckout.Shared.Notifications;
using UmbCheckout.Shared.Notifications.Basket;
using UmbCheckout.Shared.Notifications.Session;
using UmbCheckout.Stripe.Controllers.Surface;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Testing;
using UmbCheckout.Tests.Infrastructure;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Routing;
using Xunit;
using Consts = UmbCheckout.Shared.Consts;

namespace UmbCheckout.Tests.Sessions;

/// <summary>
/// Basket hooks through the real session and basket services: Loaded changes are for display, never stored or charged, and a
/// cancelled change publishes no "changed" notification.
/// </summary>
public sealed class BasketHookTests
{
    private static readonly Guid ProductKey = Guid.NewGuid();

    private readonly TestEventAggregator _events = new();
    private readonly TestEventMessagesFactory _messages = new();
    private readonly Mock<IScopedNotificationPublisher> _scoped = new();
    private readonly DefaultHttpContext _http;
    private readonly SessionService _sessions;
    private readonly BasketService _baskets;

    public BasketHookTests()
    {
        _http = new DefaultHttpContext
        {
            Session = new InMemorySession(),
            RequestServices = new ServiceCollection().AddSingleton<IEventMessagesFactory>(_messages).BuildServiceProvider()
        };
        var scope = new Mock<ICoreScope>();
        scope.SetupGet(s => s.Notifications).Returns(_scoped.Object);
        var scopes = new Mock<ICoreScopeProvider> { DefaultValue = DefaultValue.Mock };
        scopes.Setup(s => s.CreateCoreScope(It.IsAny<System.Data.IsolationLevel>(), It.IsAny<RepositoryCacheMode>(), It.IsAny<IEventDispatcher?>(),
            It.IsAny<IScopedNotificationPublisher?>(), It.IsAny<bool?>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(scope.Object);
        var basketContext = new Mock<IBasketContext>();
        basketContext.Setup(c => c.SiteRootCulture).Returns("en-GB");
        basketContext.Setup(c => c.GetCurrencyAsync()).ReturnsAsync("GBP");

        _sessions = new SessionService(new EphemeralDataProtectionProvider(), Mock.Of<IHttpContextAccessor>(a => a.HttpContext == _http), NullLogger<SessionService>.Instance,
            _events, scopes.Object, Mock.Of<IConfigurationService>(), TestLicenseService.Create(), basketContext.Object, _messages);
        _baskets = new BasketService(_sessions, NullLogger<BasketService>.Instance, _events, scopes.Object, _messages);
    }

    private static LineItem Ball(int quantity = 1) => new() { Key = ProductKey, Name = "Ball", Price = 10m, Quantity = quantity, CurrencyCode = "GBP" };

    private async Task<Basket> Stored()
    {
        using (UmbCheckoutHooks.SuppressLoaded())
        {
            return (await _sessions.Get("GBP")).Basket;
        }
    }

    [Fact]
    public async Task A_Loaded_display_change_is_not_stored_by_the_next_basket_change()
    {
        _events.On<OnSessionLoadedNotification>(n =>
        {
            foreach (var line in n.Target.Basket.LineItems)
            {
                line.Name += " (sale)";
                line.Price = 1m;
            }
        });

        await _baskets.Add(Ball());
        await _baskets.Add(Ball());

        var stored = Assert.Single((await Stored()).LineItems);
        Assert.Equal("Ball", stored.Name);
        Assert.Equal(10m, stored.Price);
        Assert.Equal(2, stored.Quantity);
    }

    [Fact]
    public async Task A_cancelled_add_publishes_no_added_notification()
    {
        _events.On<OnSessionSavingNotification>(n => n.CancelOperation(new EventMessage("Basket", "Closed", EventMessageType.Error)));

        await _baskets.Add(Ball());

        _scoped.Verify(p => p.Publish(It.IsAny<OnBasketAddedNotification>()), Times.Never);
        Assert.Empty((await Stored()).LineItems);
    }

    [Fact]
    public async Task A_cancel_with_Cancel_true_and_no_message_still_reads_as_a_cancel()
    {
        _events.On<OnSessionSavingNotification>(n => n.Cancel = true);

        await _baskets.Add(Ball());

        Assert.Equal(EventMessagesExtensions.DefaultCancellationMessage, _messages.Messages.CancellationMessage());
    }

    private StripeBasketController Checkout(Mock<IStripeSessionService> stripe)
    {
        var product = new Mock<IPublishedContent>();
        product.SetupGet(p => p.Key).Returns(ProductKey);
        var cache = new Mock<IPublishedContentCache>();
        cache.Setup(c => c.GetByIdAsync(ProductKey, It.IsAny<bool?>())).ReturnsAsync(product.Object);
        var pricing = new Mock<IStripeLineItemPricingService>();
        pricing.Setup(p => p.GetPriceAsync(product.Object)).ReturnsAsync(new LineItemPrice(10m, "GBP"));
        var repricing = new StripeBasketPricingService(cache.Object, pricing.Object, NullLogger<StripeBasketPricingService>.Instance);

        var controller = new StripeBasketController(Mock.Of<IUmbracoContextAccessor>(), Mock.Of<IUmbracoDatabaseFactory>(), ServiceContext.CreatePartial(), AppCaches.NoCache,
            Mock.Of<IProfilingLogger>(), Mock.Of<IPublishedUrlProvider>(), _baskets, stripe.Object, pricing.Object, repricing, _sessions,
            NullLogger<StripeBasketController>.Instance);
        _http.Features.Set(new UmbracoRouteValues(Mock.Of<IPublishedRequest>(r => r.PublishedContent == product.Object), new ControllerActionDescriptor()));
        controller.ControllerContext = new ControllerContext { HttpContext = _http };
        controller.TempData = new TempDataDictionary(_http, Mock.Of<ITempDataProvider>());
        return controller;
    }

    [Fact]
    public async Task A_Loaded_price_change_neither_blocks_checkout_nor_reaches_Stripe()
    {
        await _baskets.Add(Ball(quantity: 2));
        _events.On<OnSessionLoadedNotification>(n =>
        {
            foreach (var line in n.Target.Basket.LineItems)
            {
                line.Price = 1m;
                line.Quantity = 50;
            }
        });
        Basket? charged = null;
        var stripe = new Mock<IStripeSessionService>();
        stripe.Setup(s => s.CreateSessionAsync(It.IsAny<Basket>())).Callback<Basket>(b => charged = b)
            .ReturnsAsync(new Session { Id = "cs_test_1", Url = "https://checkout.stripe.com/c/pay/cs_test_1" });

        Assert.IsType<RedirectResult>(await Checkout(stripe).Checkout());

        var line = Assert.Single(charged!.LineItems);
        Assert.Equal(10m, line.Price);
        Assert.Equal(2, line.Quantity);
    }

    [Fact]
    public async Task Recording_the_checkout_session_ignores_a_handler_that_refuses_basket_saves()
    {
        // The success page clears the paid basket only if checkout recorded its session id; that bookkeeping isn't a shopper's change.
        await _baskets.Add(Ball());
        _events.On<OnSessionSavingNotification>(n => n.CancelOperation(new EventMessage("Basket", "Locked", EventMessageType.Error)));
        var stripe = new Mock<IStripeSessionService>();
        stripe.Setup(s => s.CreateSessionAsync(It.IsAny<Basket>())).ReturnsAsync(new Session { Id = "cs_test_2", Url = "https://checkout.stripe.com/c/pay/cs_test_2" });

        Assert.IsType<RedirectResult>(await Checkout(stripe).Checkout());

        Assert.Equal("cs_test_2", (await Stored()).CheckoutSessionId);
    }

    [Fact]
    public async Task A_cancelled_empty_basket_says_why()
    {
        await _baskets.Add(Ball());
        _events.On<OnSessionSavingNotification>(n => n.CancelOperation(new EventMessage("Basket", "Locked", EventMessageType.Error)));
        var controller = Checkout(new Mock<IStripeSessionService>());

        await controller.EmptyBasket();

        Assert.Equal("Locked", controller.TempData[Consts.TempData.UmbCheckoutBasketChangeCancelled]);
        Assert.Single((await Stored()).LineItems);
    }
}
