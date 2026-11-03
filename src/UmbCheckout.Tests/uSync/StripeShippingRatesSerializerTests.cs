using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Models;
using UmbCheckout.Stripe.uSync.Serializers;
using Umbraco.Cms.Core.Strings;
using uSync.Core.Serialization;
using Xunit;

namespace UmbCheckout.Tests.uSync;

public sealed class StripeShippingRatesSerializerTests
{
    [Fact]
    public async Task A_refused_import_fails_instead_of_reporting_success()
    {
        // e.g. the imported Stripe ID belongs to another rate here (its delete hasn't been imported yet): uSync must show the
        // item as failed, or checkout keeps charging the old rate without anyone knowing.
        var service = new Mock<IStripeShippingRateDatabaseService>();
        service.Setup(s => s.UpdateShippingRateAsync(It.IsAny<ShippingRate>(), It.IsAny<CancellationToken>())).ReturnsAsync((ShippingRate?)null);
        var serializer = new StripeShippingRatesSerializer(NullLogger<SyncSerializerRoot<ShippingRate>>.Instance, service.Object, Mock.Of<IShortStringHelper>());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => serializer.SaveItemAsync(new ShippingRate { Key = Guid.NewGuid(), Name = "Express", Value = "shr_1" }));

        Assert.Contains("shr_1", ex.Message);
    }
}
