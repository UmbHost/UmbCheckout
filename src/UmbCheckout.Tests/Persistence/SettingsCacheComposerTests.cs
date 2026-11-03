using Microsoft.Extensions.DependencyInjection;
using Moq;
using UmbCheckout.Shared.Notifications.Configuration;
using UmbCheckout.Stripe.Notifications;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Xunit;

namespace UmbCheckout.Tests.Persistence;

/// <summary>Quality Phase 5 review: the Saved handlers clear the caches after the outermost commit and tell the other servers.</summary>
public sealed class SettingsCacheComposerTests
{
    private static IServiceCollection Compose(IComposer composer)
    {
        var services = new ServiceCollection();
        var builder = new Mock<IUmbracoBuilder>();
        builder.SetupGet(b => b.Services).Returns(services);
        builder.Setup(b => b.WithCollectionBuilder<CacheRefresherCollectionBuilder>()).Returns(new CacheRefresherCollectionBuilder());

        composer.Compose(builder.Object);
        return services;
    }

    [Fact]
    public void The_configuration_Saved_cache_handler_is_registered()
    {
        var services = Compose(new UmbCheckout.Core.Composers.SettingsCacheComposer());

        Assert.Contains(services, d => d.ServiceType == typeof(INotificationHandler<OnConfigurationSavedNotification>)
            && d.ImplementationType == typeof(UmbCheckout.Core.Cache.UmbCheckoutConfigurationSavedCacheHandler));
    }

    [Fact]
    public void The_Stripe_settings_Saved_cache_handler_is_registered()
    {
        var services = Compose(new UmbCheckout.Stripe.Composers.SettingsCacheComposer());

        Assert.Contains(services, d => d.ServiceType == typeof(INotificationHandler<OnStripeSettingsSavedNotification>)
            && d.ImplementationType == typeof(UmbCheckout.Stripe.Cache.UmbCheckoutStripeSettingsSavedCacheHandler));
    }
}
