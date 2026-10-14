using Stripe.Checkout;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Money;
using UmbCheckout.Testing;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    /// <summary>Proves what Stripe.net puts on the wire for the price data checkout builds (stripe-mock, Docker).</summary>
    [Trait("Category", "Integration")]
    [Collection("stripe-mock")]
    public class StripeLineItemPriceStripeMockTests
    {
        private readonly StripeMockFixture _fixture;
        private readonly CapturingHandler _handler = new();

        public StripeLineItemPriceStripeMockTests(StripeMockFixture fixture) => _fixture = fixture;

        [Theory]
        [InlineData("USD", "10.99", "usd", 1099)]
        [InlineData("JPY", "500", "jpy", 500)]
        [InlineData("KWD", "1.235", "kwd", 1235)]
        [InlineData("ISK", "5", "isk", 500)]
        public async Task CreateSession_SendsTheMinorUnitAmountAndLowercaseCurrency(string basketCurrency, string price, string expectedCurrency, long expectedAmount)
        {
            var lineItem = new LineItem { Name = "Jacket", Price = decimal.Parse(price, System.Globalization.CultureInfo.InvariantCulture), CurrencyCode = basketCurrency, Quantity = 1 };
            var currency = StripeLineItemPrice.BasketCurrency(new[] { lineItem });

            await new SessionService(_fixture.CreateClient(_handler)).CreateAsync(new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = "https://example.test/success",
                LineItems = new List<SessionLineItemOptions> { new() { PriceData = StripeLineItemPrice.PriceData(lineItem, currency, "Jacket"), Quantity = 1 } }
            });

            var body = Assert.Single(_handler.RequestBodies);
            Assert.Contains($"line_items[0][price_data][unit_amount]={expectedAmount}", body);
            Assert.Contains($"line_items[0][price_data][currency]={expectedCurrency}", body);
            Assert.DoesNotContain("unit_amount_decimal", body);
        }
    }
}
