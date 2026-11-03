using Moq;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Testing;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    /// <summary>The shipping rate service against stripe-mock (Docker), through the injectable client factory.</summary>
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
            factory.Setup(f => f.CreateAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => _fixture.CreateClient(_handler));
            return new StripeShippingRateApiService(factory.Object);
        }

        [Fact]
        public async Task A_cancelled_request_stops_the_Stripe_calls()
        {
            IStripeShippingRateApiService service = Service();
            var cancelled = new CancellationToken(canceled: true);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetShippingRates(cancelled));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetShippingRate("shr_123", cancelled));

            Assert.Empty(_handler.RequestUris);
        }

        [Fact]
        public async Task GetShippingRates_ListsTheAccountsRates()
        {
            var rates = await Service().GetShippingRates(CancellationToken.None);

            Assert.NotEmpty(rates.Data);
            Assert.Contains("/v1/shipping_rates", Assert.Single(_handler.RequestUris));
        }

        [Fact]
        public async Task GetShippingRate_FetchesTheRate()
        {
            var rate = await Service().GetShippingRate("shr_123", CancellationToken.None);

            Assert.StartsWith("shr_", rate.Id);
            Assert.Contains("/v1/shipping_rates/shr_123", Assert.Single(_handler.RequestUris));
        }
    }
}
