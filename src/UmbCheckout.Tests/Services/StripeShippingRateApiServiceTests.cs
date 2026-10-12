using Moq;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Tests.Infrastructure;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    [Trait("Category", "Integration")]
    [Collection("stripe-mock")]
    public class StripeShippingRateApiServiceTests
    {
        private readonly StripeMockFixture _fixture;
        private readonly CapturingHandler _handler = new();

        public StripeShippingRateApiServiceTests(StripeMockFixture fixture) => _fixture = fixture;

        private StripeShippingRateApiService Service()
        {
            var factory = new Mock<IUmbCheckoutStripeClientFactory>();
            factory.Setup(f => f.CreateAsync()).ReturnsAsync(() => _fixture.CreateClient(_handler));
            return new StripeShippingRateApiService(factory.Object);
        }

        [Fact]
        public async Task GetShippingRates_ListsTheAccountsRates()
        {
            var rates = await Service().GetShippingRates();

            Assert.NotEmpty(rates.Data);
            Assert.Contains("/v1/shipping_rates", Assert.Single(_handler.RequestUris));
        }

        [Fact]
        public async Task GetShippingRate_FetchesTheRate()
        {
            var rate = await Service().GetShippingRate("shr_123");

            Assert.StartsWith("shr_", rate.Id);
            Assert.Contains("/v1/shipping_rates/shr_123", Assert.Single(_handler.RequestUris));
        }
    }
}
