using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Stripe.Composers;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.NotificationHandlers;
using UmbCheckout.Stripe.Notifications.Webhooks;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Xunit;

namespace UmbCheckout.Tests.Composers;

/// <summary>Sub-project B: Stripe's paid-session store replaces Core's "none paid" default, whichever composer runs first.</summary>
public sealed class PaidCheckoutSessionsComposerTests
{
    private static ServiceCollection Compose(bool coreFirst)
    {
        var services = new ServiceCollection();
        var builder = new Mock<IUmbracoBuilder>();
        builder.SetupGet(b => b.Services).Returns(services);
        void Core() => services.TryAddSingleton<IPaidCheckoutSessions>(new NoPaid());

        if (coreFirst)
        {
            Core();
        }

        new PaidCheckoutSessionsComposer().Compose(builder.Object);
        if (!coreFirst)
        {
            Core();
        }

        services.AddSingleton(Mock.Of<IStripePaidCheckoutSessions>());
        return services;
    }

    private sealed class NoPaid : IPaidCheckoutSessions
    {
        public Task<bool> IsPaidAsync(string checkoutSessionId, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Basket_reads_ask_the_Stripe_store(bool coreFirst)
    {
        var services = Compose(coreFirst);

        Assert.Single(services, d => d.ServiceType == typeof(IPaidCheckoutSessions));
        Assert.IsNotType<NoPaid>(services.BuildServiceProvider().GetRequiredService<IPaidCheckoutSessions>());
    }

    [Fact]
    public void The_webhook_notifications_are_handled()
    {
        var services = Compose(coreFirst: true);

        Assert.Contains(services, d => d.ServiceType == typeof(INotificationAsyncHandler<OnCheckoutSessionCompletedNotification>) && d.ImplementationType == typeof(RecordPaidCheckoutSessionHandler));
        Assert.Contains(services, d => d.ServiceType == typeof(INotificationAsyncHandler<OnPaymentSuccessNotification>) && d.ImplementationType == typeof(RecordPaidCheckoutSessionHandler));
    }
}
