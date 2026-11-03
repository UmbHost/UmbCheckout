using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Stripe;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Services;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    /// <summary>A Checkout session in one currency can only use shipping rates that charge in that currency.</summary>
    public class StripeDefaultShippingOptionsServiceTests
    {
        private readonly Mock<IStripeShippingRateDatabaseService> _database = new();
        private readonly Mock<IStripeShippingRateApiService> _api = new();

        private StripeDefaultShippingOptionsService Service() => new(_database.Object, _api.Object, NullLogger<StripeDefaultShippingOptionsService>.Instance);

        private void Rates(params ShippingRate[] rates)
        {
            _database.Setup(d => d.GetShippingRatesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(rates.Select(r => new UmbCheckout.Stripe.Models.ShippingRate { Value = r.Id }).ToList());
            foreach (var rate in rates)
            {
                _api.Setup(a => a.GetShippingRate(rate.Id, It.IsAny<CancellationToken>())).ReturnsAsync(rate);
            }
        }

        private static ShippingRate Rate(string id, string currency, bool active = true, string? alsoIn = null) => new()
        {
            Id = id,
            Active = active,
            FixedAmount = new ShippingRateFixedAmount
            {
                Currency = currency,
                CurrencyOptions = alsoIn == null ? null : new Dictionary<string, ShippingRateFixedAmountCurrencyOptions> { [alsoIn] = new() }
            }
        };

        private async Task<string[]> OfferedFor(string currency)
            => (await Service().GetShippingOptions(new Basket { CurrencyCode = currency })).Select(o => o.ShippingRate).ToArray();

        [Fact]
        public async Task OnlyRatesInTheBasketsCurrencyAreOffered()
        {
            Rates(Rate("shr_gbp", "gbp"), Rate("shr_usd", "usd"));
            Assert.Equal(["shr_gbp"], await OfferedFor("GBP"));
        }

        [Fact]
        public async Task ARateWithACurrencyOptionForTheBasketIsOffered()
        {
            Rates(Rate("shr_multi", "usd", alsoIn: "gbp"));
            Assert.Equal(["shr_multi"], await OfferedFor("GBP"));
        }

        [Fact]
        public async Task ArchivedRatesAreNotOffered()
        {
            Rates(Rate("shr_old", "gbp", active: false));
            Assert.Empty(await OfferedFor("GBP"));
        }

        [Fact]
        public async Task ARateStripeCannotFindIsSkipped()
        {
            Rates(Rate("shr_gbp", "gbp"));
            _database.Setup(d => d.GetShippingRatesAsync(It.IsAny<CancellationToken>())).ReturnsAsync([new UmbCheckout.Stripe.Models.ShippingRate { Value = "shr_gone" }, new UmbCheckout.Stripe.Models.ShippingRate { Value = "shr_gbp" }]);
            _api.Setup(a => a.GetShippingRate("shr_gone", It.IsAny<CancellationToken>())).ThrowsAsync(new StripeException("No such shipping rate"));

            Assert.Equal(["shr_gbp"], await OfferedFor("GBP"));
        }

        [Fact]
        public async Task AtMostFiveAreOffered()
        {
            Rates(Enumerable.Range(1, 6).Select(i => Rate($"shr_{i}", "gbp")).ToArray());
            Assert.Equal(5, (await OfferedFor("GBP")).Length);
        }
    }
}
