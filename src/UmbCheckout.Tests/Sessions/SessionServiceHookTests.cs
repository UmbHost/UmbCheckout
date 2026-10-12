using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Services;
using UmbCheckout.Shared.Models;
using UmbCheckout.Shared.Notifications;
using UmbCheckout.Shared.Notifications.Session;
using UmbCheckout.Testing;
using UmbCheckout.Tests.Infrastructure;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Xunit;

namespace UmbCheckout.Tests.Sessions;

public sealed class SessionServiceHookTests
{
    private readonly TestEventAggregator _events = new();
    private readonly TestEventMessagesFactory _messages = new();
    private readonly DefaultHttpContext _http = new() { Session = new InMemorySession() };

    private SessionService Service()
    {
        var basketContext = new Mock<IBasketContext>();
        basketContext.Setup(c => c.SiteRootCulture).Returns("en-GB");
        basketContext.Setup(c => c.GetCurrencyAsync()).ReturnsAsync("GBP");
        return new SessionService(new EphemeralDataProtectionProvider(), Mock.Of<IHttpContextAccessor>(a => a.HttpContext == _http), NullLogger<SessionService>.Instance,
            _events, new Mock<ICoreScopeProvider> { DefaultValue = DefaultValue.Mock }.Object, Mock.Of<IConfigurationService>(), TestLicenseService.Create(),
            basketContext.Object, _messages);
    }

    private static Basket Basket(params (string Name, decimal Price)[] lines) => new()
    {
        CurrencyCode = "GBP",
        LineItems = lines.Select(l => new LineItem { Key = Guid.NewGuid(), Name = l.Name, Price = l.Price, Quantity = 1, CurrencyCode = "GBP" }).ToList()
    };

    [Fact]
    public async Task A_Saving_handler_changes_the_basket_that_is_stored()
    {
        _events.On<OnSessionSavingNotification>(n => n.Target.CustomerReferenceId = "crm-42");
        var service = Service();

        await service.Update(Basket(("Ball", 2.5m)));

        Assert.Equal("crm-42", (await service.Get("GBP")).Basket.CustomerReferenceId);
    }

    [Fact]
    public async Task A_cancelled_save_keeps_the_stored_basket_and_the_message()
    {
        var service = Service();
        await service.Update(Basket(("Ball", 2.5m)));
        _events.On<OnSessionSavingNotification>(n => n.CancelOperation(new EventMessage("Basket", "Only one ball per customer", EventMessageType.Error)));

        var result = await service.Update(Basket(("Ball", 2.5m), ("Ball 2", 2.5m)));

        Assert.Single(result.Basket.LineItems);
        Assert.Single((await service.Get("GBP")).Basket.LineItems);
        Assert.Equal("Only one ball per customer", _messages.Messages.CancellationMessage());
    }

    [Fact]
    public async Task A_cancelled_first_save_leaves_an_empty_basket()
    {
        _events.On<OnSessionSavingNotification>(n => n.CancelOperation(new EventMessage("Basket", "Closed", EventMessageType.Error)));
        var service = Service();

        var result = await service.Update(Basket(("Ball", 2.5m)));

        Assert.Empty(result.Basket.LineItems);
        Assert.Equal("GBP", result.Basket.CurrencyCode);
    }

    [Fact]
    public async Task An_update_hands_the_handler_the_stored_basket()
    {
        var service = Service();
        await service.Update(Basket(("Ball", 2.5m)));
        int? before = null;
        _events.On<OnSessionSavingNotification>(n => before = n.Existing?.LineItems.Count());

        await service.Update(Basket(("Ball", 2.5m), ("Bat", 10m)));

        Assert.Equal(1, before);
    }

    [Fact]
    public async Task A_Loaded_handler_changes_what_the_caller_gets_once_per_read()
    {
        var service = Service();
        await service.Update(Basket(("Ball", 2.5m)));
        _events.On<OnSessionLoadedNotification>(n => n.Target.Basket.MetaData["shown"] = "yes");

        var session = await service.Get("en-GB", "GBP");

        Assert.Equal("yes", session.Basket.MetaData["shown"]);
        Assert.Single(_events.Published.OfType<OnSessionLoadedNotification>());
    }

    [Fact]
    public async Task Loaded_sees_the_basket_restored_when_the_scope_closes()
    {
        // The add-on restores a cookie or database basket from OnSessionGetNotification, which runs as the scope closes.
        OnSessionGetNotification? get = null;
        var publisher = new Mock<IScopedNotificationPublisher>();
        publisher.Setup(p => p.Publish(It.IsAny<OnSessionGetNotification>())).Callback<INotification>(n => get = (OnSessionGetNotification)n);
        var scope = new Mock<ICoreScope>();
        scope.SetupGet(s => s.Notifications).Returns(publisher.Object);
        scope.Setup(s => s.Dispose()).Callback(() =>
        {
            if (get?.Session != null)
            {
                get.Session.Basket = Basket(("Restored", 1m));
            }
        });
        var scopes = new Mock<ICoreScopeProvider> { DefaultValue = DefaultValue.Mock };
        scopes.Setup(s => s.CreateCoreScope(It.IsAny<System.Data.IsolationLevel>(), It.IsAny<RepositoryCacheMode>(), It.IsAny<IEventDispatcher?>(),
            It.IsAny<IScopedNotificationPublisher?>(), It.IsAny<bool?>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(scope.Object);
        var basketContext = new Mock<IBasketContext>();
        basketContext.Setup(c => c.SiteRootCulture).Returns("en-GB");
        var service = new SessionService(new EphemeralDataProtectionProvider(), Mock.Of<IHttpContextAccessor>(a => a.HttpContext == _http), NullLogger<SessionService>.Instance,
            _events, scopes.Object, Mock.Of<IConfigurationService>(), TestLicenseService.Create(), basketContext.Object, _messages);
        string? seen = null;
        _events.On<OnSessionLoadedNotification>(n => seen = n.Target.Basket.LineItems.SingleOrDefault()?.Name);

        await service.Get("en-GB", "GBP");

        Assert.Equal("Restored", seen);
    }

    [Fact]
    public async Task Every_Get_overload_publishes_one_Loaded()
    {
        var service = Service();

        await service.Get();
        await service.Get("GBP");
        await service.Get("en-GB", "GBP");

        Assert.Equal(3, _events.Published.OfType<OnSessionLoadedNotification>().Count());
    }
}
