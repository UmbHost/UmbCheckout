using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Stripe;
using UmbCheckout.Stripe.Controllers.BackOffice.Api;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Tests.Helpers;
using Umbraco.Cms.Core.Services;
using Xunit;

namespace UmbCheckout.Tests.Controllers
{
    /// <summary>
    /// The Stripe shipping-rate API adds a money object, <c>fixedAmountMoney</c>, to each rate (converted on the server). The rest of
    /// the response is unchanged: the same serializer and options as before.
    /// </summary>
    public class StripeShippingRatesApiControllerTests
    {
        private static readonly JsonSerializerOptions OldOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private readonly Mock<IStripeShippingRateApiService> _api = new();

        private static ShippingRate Rate(string id, long amount, string currency) => new()
        {
            Id = id,
            DisplayName = "Next day",
            Active = true,
            FixedAmount = new ShippingRateFixedAmount { Amount = amount, Currency = currency },
            DeliveryEstimate = new ShippingRateDeliveryEstimate
            {
                Minimum = new ShippingRateDeliveryEstimateMinimum { Value = 1, Unit = "business_day" },
                Maximum = new ShippingRateDeliveryEstimateMaximum { Value = 2, Unit = "business_day" }
            }
        };

        private StripeShippingRatesApiController Controller() => new(NullLogger<StripeShippingRatesApiController>.Instance,
            Mock.Of<IStripeShippingRateDatabaseService>(), _api.Object, Mock.Of<ILocalizedTextService>());

        [Theory]
        [InlineData(500, "jpy", 500)]
        [InlineData(495, "gbp", 4.95)]
        public async Task GetStripeShippingRate_AddsTheFixedAmountInMajorUnits(long minorUnits, string currency, double major)
        {
            var rate = Rate("shr_1", minorUnits, currency);
            _api.Setup(a => a.GetShippingRate("shr_1", It.IsAny<CancellationToken>())).ReturnsAsync(rate);

            var body = JsonResultAssert.Body(await Controller().GetStripeShippingRate("shr_1"));

            var money = body["fixedAmountMoney"];
            Assert.NotNull(money);
            Assert.Equal(minorUnits, money.Required()["minorUnits"].Required().GetValue<long>());
            Assert.Equal((decimal)major, money["amount"].Required().GetValue<decimal>());
            Assert.Equal(currency, money["currency"].Required().GetValue<string>());
        }

        [Fact]
        public async Task GetStripeShippingRate_KeepsEveryExistingField()
        {
            var rate = Rate("shr_1", 495, "gbp");
            _api.Setup(a => a.GetShippingRate("shr_1", It.IsAny<CancellationToken>())).ReturnsAsync(rate);
            var before = JsonNode.Parse(JsonSerializer.Serialize(rate, rate.GetType(), OldOptions));

            var after = JsonResultAssert.Body(await Controller().GetStripeShippingRate("shr_1"));

            // Stripe.net 50 carries System.Text.Json names, so the existing fields are snake_case whatever the naming policy.
            Assert.Equal(495, after["fixed_amount"].Required()["amount"].Required().GetValue<long>());
            Assert.Equal("Next day", after["display_name"].Required().GetValue<string>());
            JsonResultAssert.Additive(before, after, "fixedAmountMoney");
        }

        [Fact]
        public async Task GetStripeShippingRates_AddsMoneyToEachRate_AndKeepsEveryExistingField()
        {
            var list = new StripeList<ShippingRate> { Data = new List<ShippingRate> { Rate("shr_jpy", 500, "jpy"), Rate("shr_gbp", 495, "gbp") } };
            _api.Setup(a => a.GetShippingRates(It.IsAny<CancellationToken>())).ReturnsAsync(list);
            var before = JsonNode.Parse(JsonSerializer.Serialize(list, list.GetType(), OldOptions));

            var after = JsonResultAssert.Body(await Controller().GetStripeShippingRates());

            var data = after["data"].Required().AsArray();
            Assert.Equal(500m, data[0].Required()["fixedAmountMoney"].Required()["amount"].Required().GetValue<decimal>());
            Assert.Equal(4.95m, data[1].Required()["fixedAmountMoney"].Required()["amount"].Required().GetValue<decimal>());
            JsonResultAssert.Additive(before, after, "fixedAmountMoney");
        }

        [Fact]
        public async Task GetStripeShippingRate_WithoutAFixedAmount_AddsNothing()
        {
            var rate = new ShippingRate { Id = "shr_free", DisplayName = "Free" };
            _api.Setup(a => a.GetShippingRate("shr_free", It.IsAny<CancellationToken>())).ReturnsAsync(rate);
            var before = JsonNode.Parse(JsonSerializer.Serialize(rate, rate.GetType(), OldOptions));

            var after = JsonResultAssert.Body(await Controller().GetStripeShippingRate("shr_free"));

            JsonResultAssert.Additive(before, after);
        }
    

        [Fact]
        public async Task UpdateShippingRate_ARefusedUpdateIsABadRequest()
        {
            // e.g. the new Stripe ID belongs to another rate: the service refuses with null, and the editor must not see "saved".
            var database = new Mock<IStripeShippingRateDatabaseService>();
            database.Setup(d => d.UpdateShippingRate(It.IsAny<UmbCheckout.Stripe.Models.ShippingRate>())).ReturnsAsync((UmbCheckout.Stripe.Models.ShippingRate?)null);
            var controller = new StripeShippingRatesApiController(NullLogger<StripeShippingRatesApiController>.Instance, database.Object, _api.Object, Mock.Of<ILocalizedTextService>());

            Assert.IsType<BadRequestResult>(await controller.UpdateShippingRate(new UmbCheckout.Stripe.Models.ShippingRate { Name = "One", Value = "shr_2" }));
        }
    }
}
