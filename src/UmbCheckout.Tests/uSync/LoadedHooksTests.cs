using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Models;
using UmbCheckout.Shared.Notifications;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Models;
using UmbCheckout.Stripe.uSync.Serializers;
using UmbCheckout.uSync.Serializers;
using Umbraco.Cms.Core.Strings;
using uSync.Core.Serialization;
using Xunit;

namespace UmbCheckout.Tests.uSync;

/// <summary>uSync moves stored data between environments: it reads without Loaded, so a site's display changes aren't exported or saved back.</summary>
public sealed class LoadedHooksTests
{
    [Fact]
    public async Task The_configuration_serializer_reads_stored_data()
    {
        bool? suppressed = null;
        var service = new Mock<IConfigurationService>();
        service.Setup(s => s.GetConfigurationAsync(It.IsAny<CancellationToken>())).Callback(() => suppressed = UmbCheckoutHooks.LoadedSuppressed).ReturnsAsync(new UmbCheckoutConfiguration());

        await new ConfigurationSerializer(NullLogger<SyncSerializerRoot<UmbCheckoutConfiguration>>.Instance, service.Object).FindItemAsync(Guid.NewGuid());

        Assert.True(suppressed);
    }

    [Fact]
    public async Task The_Stripe_settings_serializer_reads_stored_data()
    {
        bool? suppressed = null;
        var service = new Mock<IStripeSettingsService>();
        service.Setup(s => s.GetStripeSettingsAsync(It.IsAny<CancellationToken>())).Callback(() => suppressed = UmbCheckoutHooks.LoadedSuppressed).ReturnsAsync(new UmbCheckoutStripeSettings());

        await new StripeSettingsSerializer(NullLogger<SyncSerializerRoot<UmbCheckoutStripeSettings>>.Instance, service.Object).FindItemAsync(Guid.NewGuid());

        Assert.True(suppressed);
    }

    [Fact]
    public async Task The_shipping_rate_serializer_reads_stored_data()
    {
        bool? suppressed = null;
        var service = new Mock<IStripeShippingRateDatabaseService>();
        service.Setup(s => s.GetShippingRateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Callback(() => suppressed = UmbCheckoutHooks.LoadedSuppressed).ReturnsAsync(new ShippingRate());

        await new StripeShippingRatesSerializer(NullLogger<SyncSerializerRoot<ShippingRate>>.Instance, service.Object, Mock.Of<IShortStringHelper>()).FindItemAsync(Guid.NewGuid());

        Assert.True(suppressed);
    }
}
