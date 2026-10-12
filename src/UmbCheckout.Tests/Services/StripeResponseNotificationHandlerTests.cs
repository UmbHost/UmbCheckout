using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Stripe;
using Stripe.Checkout;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.NotificationHandlers;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Routing;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    /// <summary>
    /// After payment the paid basket is cleared. The handler runs while routing, before the page (and so its culture) is
    /// known, so the basket's culture and currency travel in the Stripe session's metadata. It acts only on the configured
    /// success page, for a well-formed checkout session id, and never lets a Stripe failure break the page.
    /// </summary>
    public class StripeResponseNotificationHandlerTests
    {
        private readonly Mock<IStripeSessionService> _stripe = new();
        private readonly Mock<ISessionService> _sessions = new();
        private readonly Mock<IUmbCheckoutSuccessPageMatcher> _matcher = new();

        private Task Request(string url, bool isSuccessPage = true)
        {
            _matcher.Setup(m => m.IsSuccessPageAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).ReturnsAsync(isSuccessPage);
            var request = new Mock<IPublishedRequestBuilder>();
            request.Setup(r => r.Uri).Returns(new Uri(url));
            return new StripeResponseNotificationHandler(_stripe.Object, _sessions.Object, _matcher.Object, NullLogger<StripeResponseNotificationHandler>.Instance)
                .HandleAsync(new RoutingRequestNotification(request.Object), CancellationToken.None);
        }

        private Task Succeed(Session session)
        {
            _stripe.Setup(s => s.GetSessionAsync("cs_test_1")).ReturnsAsync(session);
            return Request("https://shop.test/success/?session_id=cs_test_1&success=true");
        }

        private static Dictionary<string, string> BasketMetadata() => new() { ["umbcheckout_culture"] = "en-GB", ["umbcheckout_currency"] = "GBP" };

        [Fact]
        public async Task ClearsThePaidBasketsCultureAndCurrency()
        {
            await Succeed(new Session { Status = "complete", PaymentStatus = "paid", Currency = "gbp", Metadata = BasketMetadata() });

            _sessions.Verify(s => s.Clear("en-GB", "GBP"), Times.Once);
        }

        [Fact]
        public async Task ASessionWithoutBasketMetadataClearsItsCurrencyForTheRequest()
        {
            // Checkout sessions created before the upgrade carry no basket metadata.
            await Succeed(new Session { Status = "complete", PaymentStatus = "paid", Currency = "gbp", Metadata = new Dictionary<string, string>() });

            _sessions.Verify(s => s.Clear("GBP"), Times.Once);
        }

        [Fact]
        public async Task AnIncompleteSessionClearsNothing()
        {
            await Succeed(new Session { Status = "open", Currency = "gbp", Metadata = new Dictionary<string, string>() });

            _sessions.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Any_other_page_with_the_query_string_does_nothing()
        {
            await Request("https://shop.test/about/?session_id=cs_test_1&success=true", isSuccessPage: false);

            _stripe.VerifyNoOtherCalls();
            _sessions.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("pi_123")]
        [InlineData("cs_test_1' OR 1=1")]
        [InlineData("cs_test_")]
        public async Task A_session_id_that_is_not_a_checkout_session_is_ignored(string sessionId)
        {
            await Request($"https://shop.test/success/?session_id={Uri.EscapeDataString(sessionId)}&success=true");

            _stripe.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task A_Stripe_error_is_logged_and_the_page_still_renders()
        {
            _stripe.Setup(s => s.GetSessionAsync("cs_test_1")).ThrowsAsync(new StripeException("No such checkout.session"));

            await Request("https://shop.test/success/?session_id=cs_test_1&success=true"); // no exception escapes

            _sessions.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task A_Stripe_timeout_is_logged_and_the_page_still_renders()
        {
            // Stripe.net rethrows an HttpClient timeout as a raw TaskCanceledException; the customer has just paid.
            _stripe.Setup(s => s.GetSessionAsync("cs_test_1")).ThrowsAsync(new TaskCanceledException("The request was canceled due to the configured HttpClient.Timeout"));

            await Request("https://shop.test/success/?session_id=cs_test_1&success=true"); // no exception escapes

            _sessions.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task A_failure_while_matching_the_success_page_is_logged_and_the_page_still_renders()
        {
            var request = new Mock<IPublishedRequestBuilder>();
            request.Setup(r => r.Uri).Returns(new Uri("https://shop.test/success/?session_id=cs_test_1&success=true"));
            _matcher.Setup(m => m.IsSuccessPageAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("database down"));

            await new StripeResponseNotificationHandler(_stripe.Object, _sessions.Object, _matcher.Object, NullLogger<StripeResponseNotificationHandler>.Instance)
                .HandleAsync(new RoutingRequestNotification(request.Object), CancellationToken.None); // no exception escapes

            _stripe.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("paid")]
        [InlineData("no_payment_required")]
        [InlineData("unpaid")] // delayed methods (Bacs, SEPA): checkout is committed; the bank's result arrives by webhook
        public async Task A_complete_session_clears_the_basket_whatever_its_payment_status(string paymentStatus)
        {
            await Succeed(new Session { Status = "complete", PaymentStatus = paymentStatus, Currency = "gbp", Metadata = BasketMetadata() });

            _sessions.Verify(s => s.Clear("en-GB", "GBP"), Times.Once);
        }
    }
}
