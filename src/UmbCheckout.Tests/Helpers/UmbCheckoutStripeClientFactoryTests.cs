using Microsoft.Extensions.Options;
using Moq;
using Stripe;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Models;
using UmbCheckout.Stripe.Services;
using Xunit;

namespace UmbCheckout.Tests.Helpers
{
    public class UmbCheckoutStripeClientFactoryTests
    {
        private static UmbCheckoutStripeClientFactory Factory(UmbCheckoutStripeSettings? settings)
        {
            var options = new Mock<IOptionsMonitor<StripeSettings>>();
            options.Setup(o => o.CurrentValue).Returns(new StripeSettings
            {
                Test = new StripeApiDetails { ApiKey = "sk_test_key" },
                Live = new StripeApiDetails { ApiKey = "sk_live_key" }
            });
            var settingsService = new Mock<IStripeSettingsService>();
            settingsService.Setup(s => s.GetStripeSettings()).ReturnsAsync(settings);
            return new UmbCheckoutStripeClientFactory(options.Object, settingsService.Object);
        }

        [Fact]
        public async Task UsesTheTestKey_WhenLiveDetailsAreOff()
            => Assert.Equal("sk_test_key", ((StripeClient)await Factory(new UmbCheckoutStripeSettings { UseLiveApiDetails = false }).CreateAsync()).ApiKey);

        [Fact]
        public async Task UsesTheLiveKey_WhenLiveDetailsAreOn()
            => Assert.Equal("sk_live_key", ((StripeClient)await Factory(new UmbCheckoutStripeSettings { UseLiveApiDetails = true }).CreateAsync()).ApiKey);

        [Fact]
        public async Task UsesTheTestKey_WhenNoSettingsAreSaved()
            // Never fall through to live keys by accident.
            => Assert.Equal("sk_test_key", ((StripeClient)await Factory(null).CreateAsync()).ApiKey);
    }
}
