using Stripe;
using UmbCheckout.Stripe.Models.Api;
using Xunit;

namespace UmbCheckout.Tests.Api
{
    /// <summary>The "Stripe Shipping Rate Details" box of v13's shipping-rate page.</summary>
    public class StripeShippingRateResponseModelTests
    {
        private static ShippingRate Rate(bool livemode = false) => new()
        {
            Id = "shr_123",
            DisplayName = "Next day",
            Active = true,
            Livemode = livemode,
            FixedAmount = new ShippingRateFixedAmount { Amount = 495, Currency = "gbp" },
            DeliveryEstimate = new ShippingRateDeliveryEstimate
            {
                Minimum = new ShippingRateDeliveryEstimateMinimum { Value = 1, Unit = "business_day" },
                Maximum = new ShippingRateDeliveryEstimateMaximum { Value = 2, Unit = "business_day" }
            }
        };

        [Fact]
        public void Amount_IsMoneyInMajorUnits()
        {
            var model = StripeShippingRateResponseModel.From(Rate());

            Assert.Equal(495, model.Amount!.MinorUnits);
            Assert.Equal(4.95m, model.Amount.Amount);
            Assert.Equal("gbp", model.Amount.Currency);
        }

        [Fact]
        public void Amount_IsNull_WhenTheRateHasNoFixedAmount()
            => Assert.Null(StripeShippingRateResponseModel.From(new ShippingRate { Id = "shr_1", DisplayName = "Free" }).Amount);

        [Fact]
        public void DeliveryEstimate_IsMapped()
        {
            var model = StripeShippingRateResponseModel.From(Rate());

            Assert.Equal(1, model.DeliveryEstimateMinimum!.Value);
            Assert.Equal("business_day", model.DeliveryEstimateMinimum.Unit);
            Assert.Equal(2, model.DeliveryEstimateMaximum!.Value);
        }

        [Theory]
        [InlineData(false, "https://dashboard.stripe.com/test/shipping-rates/shr_123")]
        [InlineData(true, "https://dashboard.stripe.com/shipping-rates/shr_123")]
        public void DashboardUrl_FollowsTheRatesMode(bool livemode, string expected)
            // v13 linked to the test or live dashboard depending on the mode.
            => Assert.Equal(expected, StripeShippingRateResponseModel.From(Rate(livemode)).DashboardUrl);
    }
}
