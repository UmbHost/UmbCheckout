using Moq;
using Stripe.Checkout;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.NotificationHandlers;
using UmbCheckout.Stripe.Services;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Routing;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    /// <summary>
    /// After payment the paid basket is cleared. The handler runs while routing, before the page (and so its culture) is
    /// known, so the basket's culture and currency travel in the Stripe session's metadata.
    /// </summary>
    public class StripeResponseNotificationHandlerTests
    {
        private readonly Mock<IStripeSessionService> _stripe = new();
        private readonly Mock<ISessionService> _sessions = new();

        private Task Succeed(Session session)
        {
            _stripe.Setup(s => s.GetSessionAsync("cs_test_1")).ReturnsAsync(session);
            var request = new Mock<IPublishedRequestBuilder>();
            request.Setup(r => r.Uri).Returns(new Uri("https://shop.test/success/?session_id=cs_test_1&success=true"));
            return new StripeResponseNotificationHandler(_stripe.Object, _sessions.Object).HandleAsync(new RoutingRequestNotification(request.Object), CancellationToken.None);
        }

        [Fact]
        public async Task ClearsThePaidBasketsCultureAndCurrency()
        {
            await Succeed(new Session
            {
                Status = "complete",
                Currency = "gbp",
                Metadata = new Dictionary<string, string> { ["umbcheckout_culture"] = "en-GB", ["umbcheckout_currency"] = "GBP" }
            });

            _sessions.Verify(s => s.Clear("en-GB", "GBP"), Times.Once);
            _sessions.Verify(s => s.Clear(), Times.Never);
        }

        [Fact]
        public async Task ABasketWithNoCultureIsClearedAsSuch()
        {
            // A site without domains: the basket's culture is null (stored as an empty value).
            await Succeed(new Session { Status = "complete", Currency = "usd", Metadata = new Dictionary<string, string> { ["umbcheckout_culture"] = "", ["umbcheckout_currency"] = "USD" } });

            _sessions.Verify(s => s.Clear(null, "USD"), Times.Once);
        }

        [Fact]
        public async Task ASessionWithoutBasketMetadataClearsItsCurrencyForTheRequest()
        {
            // Checkout sessions created before the upgrade carry no basket metadata.
            await Succeed(new Session { Status = "complete", Currency = "gbp", Metadata = new Dictionary<string, string>() });

            _sessions.Verify(s => s.Clear("GBP"), Times.Once);
        }

        [Fact]
        public async Task AnIncompleteSessionClearsNothing()
        {
            await Succeed(new Session { Status = "open", Currency = "gbp", Metadata = new Dictionary<string, string>() });

            _sessions.VerifyNoOtherCalls();
        }

        [Fact]
        public void CheckoutRecordsWhichBasketTheSessionPaysFor()
        {
            var metadata = StripeSessionService.BasketMetadata(new Basket { SiteRootCulture = "en-GB", CurrencyCode = "GBP" });

            Assert.Equal("en-GB", metadata["umbcheckout_culture"]);
            Assert.Equal("GBP", metadata["umbcheckout_currency"]);
        }

        [Fact]
        public void ABasketSavedBeforeBasketsWerePerCurrencyRecordsItsLinesCurrency()
        {
            var metadata = StripeSessionService.BasketMetadata(new Basket { LineItems = new List<LineItem> { new() { CurrencyCode = "usd" } } });

            Assert.Equal(("", "USD"), (metadata["umbcheckout_culture"], metadata["umbcheckout_currency"]));
        }
    }
}
