using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Stripe.Checkout;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Models;
using UmbCheckout.Stripe.Notifications;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Xunit;

namespace UmbCheckout.Tests.Services;

/// <summary>Session-option handlers run while the options are built, before they are sent to Stripe, so what they change is used.</summary>
public sealed class StripeSessionOptionsHookTests
{
    private readonly Mock<IEventAggregator> _events = new();
    private readonly Guid _key = Guid.NewGuid();

    public StripeSessionOptionsHookTests()
    {
        // The friendly HasValue/Value extensions the options builder uses resolve these from Umbraco's static service provider.
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IPublishedValueFallback>());
        services.AddSingleton(Mock.Of<IVariationContextAccessor>());
        services.AddSingleton(Mock.Of<IPublishedSnapshotAccessor>());
        StaticServiceProvider.Instance = services.BuildServiceProvider();
    }

    /// <summary>StripeSessionService is internal in v13: created and called through reflection.</summary>
    private static readonly Type ServiceType = typeof(IStripeSessionService).Assembly.GetType("UmbCheckout.Stripe.Services.StripeSessionService", throwOnError: true)!;

    private object Service()
    {
        var product = new Mock<IPublishedContent> { DefaultValue = DefaultValue.Mock };
        product.Setup(p => p.Key).Returns(_key);
        product.Setup(p => p.Name).Returns("Ping Pong Ball");
        var content = new Mock<IPublishedContentCache>();
        content.Setup(c => c.GetById(_key)).Returns(product.Object);
        var snapshot = new Mock<IPublishedSnapshot>();
        snapshot.Setup(s => s.Content).Returns(content.Object);
        var published = snapshot.Object;
        var snapshotAccessor = new Mock<IPublishedSnapshotAccessor>();
        snapshotAccessor.Setup(a => a.TryGetPublishedSnapshot(out published)).Returns(true);

        // The constructor runs a live licence check: skip it and set the fields the options builder reads, matched by type.
        var service = RuntimeHelpers.GetUninitializedObject(ServiceType);
        var dependencies = new object[]
        {
            snapshotAccessor.Object, new Mock<ICoreScopeProvider> { DefaultValue = DefaultValue.Mock }.Object, _events.Object,
            Activator.CreateInstance(typeof(NullLogger<>).MakeGenericType(ServiceType))!, Mock.Of<IConfigurationService>(), Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IStripeShippingOptionsService>(), Mock.Of<IStripeSettingsService>(), new StripeSettings()
        };
        foreach (var field in ServiceType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
        {
            var dependency = dependencies.FirstOrDefault(field.FieldType.IsInstanceOfType);
            if (dependency != null)
            {
                field.SetValue(service, dependency);
            }
        }

        return service;
    }

    private Basket Basket() => new()
    {
        LineItems = new List<LineItem> { new() { Key = _key, Name = "Ping Pong Ball", Description = "Table tennis ball", Price = 2m, Quantity = 1 } }
    };

    private static async Task<SessionCreateOptions> BuildOptions(object service, Basket basket)
    {
        var method = ServiceType.GetMethod("CreateSessionOptions", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (await (Task<SessionCreateOptions?>)method.Invoke(service, new object[] { basket })!)!;
    }

    private void On<TNotification>(Action<TNotification> handler) where TNotification : INotification
        => _events.Setup(e => e.PublishAsync(It.IsAny<TNotification>(), It.IsAny<CancellationToken>()))
            .Callback<TNotification, CancellationToken>((notification, _) => handler(notification))
            .Returns(Task.CompletedTask);

    [Fact]
    public async Task What_handlers_change_is_in_the_options_sent_to_Stripe()
    {
        // The notifications were queued until the scope ended, after the session had been created: changes were ignored.
        On<OnProviderSessionOptionsLineItemAdding>(n => n.LineItem.PriceData.UnitAmountDecimal = 150);
        On<OnProviderSessionOptionsStarted>(n => n.Options.Metadata = new Dictionary<string, string> { ["site_order"] = "42" });
        On<OnProviderSessionOptionsCreated>(n => n.Options.ClientReferenceId = "order-42");

        var options = await BuildOptions(Service(), Basket());

        Assert.Equal(150, options.LineItems.Single().PriceData.UnitAmountDecimal);
        Assert.Equal("42", options.Metadata["site_order"]);
        Assert.Equal("order-42", options.ClientReferenceId);
    }

    [Fact]
    public async Task A_throwing_handler_stops_checkout()
    {
        _events.Setup(e => e.PublishAsync(It.IsAny<OnProviderSessionOptionsCreated>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("a site rule refused this basket"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => BuildOptions(Service(), Basket()));
    }
}
