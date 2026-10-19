using Microsoft.Extensions.DependencyInjection;
using Moq;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Xunit;

namespace UmbCheckout.Tests.uSync;

/// <summary>uSync exports on save without blocking a thread, and every handler is registered for the notifications it handles.</summary>
public sealed class HandlerAsyncTests
{
    public static TheoryData<Type> Handlers() => new()
    {
        typeof(UmbCheckout.Stripe.uSync.Handlers.ShippingRateHandler),
        typeof(UmbCheckout.Stripe.uSync.Handlers.StripeSettingsHandler),
        typeof(UmbCheckout.uSync.Handlers.ConfigurationHandler),
    };

    [Theory]
    [MemberData(nameof(Handlers))]
    public void Handlers_are_async(Type handler)
    {
        var interfaces = handler.GetInterfaces().Where(i => i.IsGenericType).Select(i => i.GetGenericTypeDefinition()).ToList();

        Assert.Contains(typeof(INotificationAsyncHandler<>), interfaces);
        Assert.DoesNotContain(typeof(INotificationHandler<>), interfaces);
    }

    [Theory]
    [MemberData(nameof(Handlers))]
    public void Every_notification_a_handler_handles_is_registered(Type handler)
    {
        // The Stripe settings handler was never registered, so saving the settings never wrote a uSync file.
        var services = new ServiceCollection();
        var builder = new Mock<IUmbracoBuilder>();
        builder.SetupGet(b => b.Services).Returns(services);
        foreach (var composer in handler.Assembly.GetTypes().Where(t => typeof(Umbraco.Cms.Core.Composing.IComposer).IsAssignableFrom(t) && !t.IsAbstract))
        {
            ((Umbraco.Cms.Core.Composing.IComposer)Activator.CreateInstance(composer).Required()).Compose(builder.Object);
        }

        var handled = handler.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationAsyncHandler<>))
            .Select(i => i.GetGenericArguments()[0]);

        Assert.All(handled, notification => Assert.Contains(services, d =>
            d.ServiceType == typeof(INotificationAsyncHandler<>).MakeGenericType(notification) && d.ImplementationType == handler));
    }
}
