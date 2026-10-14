using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Stripe;
using UmbCheckout.Stripe.Controllers.BackOffice.Api;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Tests.Helpers;
using Umbraco.Cms.Core.Services;
using Xunit;

namespace UmbCheckout.Tests.Controllers;

/// <summary>
/// Pins the JSON names the shipping-rate screens read. Stripe.net 47+ carries its own System.Text.Json names, so Stripe objects reach
/// the screen in snake_case (<c>display_name</c>, <c>delivery_estimate</c>, <c>fixed_amount</c>) whatever the naming policy, a Stripe
/// list is an object with <c>data</c>, and the money object UmbCheckout adds is camelCase (<c>fixedAmountMoney</c>).
/// </summary>
public sealed class StripeShippingRateJsonNamesTests
{
    private static ShippingRate Rate() => new()
    {
        Id = "shr_1",
        DisplayName = "Next day",
        Livemode = true,
        FixedAmount = new ShippingRateFixedAmount { Amount = 495, Currency = "gbp" },
        DeliveryEstimate = new ShippingRateDeliveryEstimate
        {
            Minimum = new ShippingRateDeliveryEstimateMinimum { Value = 1, Unit = "business_day" },
            Maximum = new ShippingRateDeliveryEstimateMaximum { Value = 2, Unit = "business_day" }
        }
    };

    private static void Has(JsonNode? node, params string[] paths)
    {
        foreach (var path in paths)
        {
            var current = node;
            foreach (var part in path.Split('.'))
            {
                current = Assert.IsType<JsonObject>(current)[part];
                Assert.True(current is not null, $"{path} is missing");
            }
        }
    }

    [Fact]
    public async Task The_shipping_rate_screens_read_these_names()
    {
        var api = new Mock<IStripeShippingRateApiService>();
        api.Setup(a => a.GetShippingRates(It.IsAny<CancellationToken>())).ReturnsAsync(new StripeList<ShippingRate> { Data = new List<ShippingRate> { Rate() } });
        api.Setup(a => a.GetShippingRate("shr_1", It.IsAny<CancellationToken>())).ReturnsAsync(Rate());
        var controller = new StripeShippingRatesApiController(NullLogger<StripeShippingRatesApiController>.Instance,
            Mock.Of<IStripeShippingRateDatabaseService>(), api.Object, Mock.Of<ILocalizedTextService>());

        var list = JsonResultAssert.Body(await controller.GetStripeShippingRates());
        Has(list["data"].Required()[0], "id", "display_name");

        var rate = JsonResultAssert.Body(await controller.GetStripeShippingRate("shr_1"));
        Has(rate, "id", "display_name", "livemode", "fixed_amount.amount", "fixed_amount.currency", "fixedAmountMoney.amount",
            "delivery_estimate.minimum.value", "delivery_estimate.minimum.unit", "delivery_estimate.maximum.value", "delivery_estimate.maximum.unit");
    }
}
