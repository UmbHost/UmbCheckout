using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Models;
using UmbCheckout.Stripe.uSync.Serializers;
using Umbraco.Cms.Core.Strings;
using uSync.Core.Serialization;
using Xunit;

namespace UmbCheckout.Tests.uSync
{
    public class StripeShippingRatesSerializerTests
    {
        private readonly Mock<IStripeShippingRateDatabaseService> _service = new();

        private StripeShippingRatesSerializer Serializer() =>
            new(NullLogger<SyncSerializerRoot<ShippingRate>>.Instance, _service.Object, Mock.Of<IShortStringHelper>());

        [Fact]
        public void A_refused_import_fails_instead_of_reporting_success()
        {
            // e.g. the imported Stripe ID belongs to another rate here (its delete hasn't been imported yet): uSync must show the
            // item as failed, or checkout keeps charging the old rate without anyone knowing.
            _service.Setup(s => s.UpdateShippingRate(It.IsAny<ShippingRate>())).ReturnsAsync((ShippingRate?)null);

            var ex = Assert.Throws<InvalidOperationException>(() => Serializer().SaveItem(new ShippingRate { Key = Guid.NewGuid(), Name = "Express", Value = "shr_1" }));

            Assert.Contains("shr_1", ex.Message);
        }

        [Fact]
        public void A_failed_save_fails_the_import()
        {
            // The save was fire-and-forget: its failure was never seen.
            _service.Setup(s => s.UpdateShippingRate(It.IsAny<ShippingRate>())).ThrowsAsync(new InvalidOperationException("database down"));

            Assert.Throws<InvalidOperationException>(() => Serializer().SaveItem(new ShippingRate { Key = Guid.NewGuid(), Name = "Express", Value = "shr_1" }));
        }
    }
}
