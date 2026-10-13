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

        public StripeResponseNotificationHandlerTests() => CurrentBasketCheckedOutAs("cs_test_1");

        /// <summary>The visitor's current en-GB GBP basket, recorded at checkout against a Stripe checkout session (or none).</summary>
        private void CurrentBasketCheckedOutAs(string? checkoutSessionId)
            => _sessions.Setup(s => s.Get("en-GB", "GBP", It.IsAny<CancellationToken>())).ReturnsAsync(new UmbCheckout.Shared.Models.UmbCheckoutSession
            {
                SiteRootCulture = "en-GB",
                Basket = new UmbCheckout.Shared.Models.Basket { SiteRootCulture = "en-GB", CurrencyCode = "GBP", CheckoutSessionId = checkoutSessionId }
            });

        private Task Request(string url, bool isSuccessPage = true)
        {
            _matcher.Setup(m => m.IsSuccessPageAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).ReturnsAsync(isSuccessPage);
            var request = new Mock<IPublishedRequestBuilder>();
            request.Setup(r => r.Uri).Returns(new Uri(url));
            return new StripeResponseNotificationHandler(_stripe.Object, _sessions.Object, _matcher.Object, NullLogger<StripeResponseNotificationHandler>.Instance, _ => "en-GB")
                .HandleAsync(new RoutingRequestNotification(request.Object), CancellationToken.None);
        }

        private Task Succeed(Session session)
        {
            session.Id ??= "cs_test_1";
            _stripe.Setup(s => s.GetSessionAsync("cs_test_1", It.IsAny<CancellationToken>())).ReturnsAsync(session);
            return Request("https://shop.test/success/?session_id=cs_test_1&success=true");
        }

        private static Dictionary<string, string> BasketMetadata() => new() { ["umbcheckout_culture"] = "en-GB", ["umbcheckout_currency"] = "GBP" };

        [Fact]
        public async Task ClearsThePaidBasketsCultureAndCurrency()
        {
            await Succeed(new Session { Status = "complete", PaymentStatus = "paid", Currency = "gbp", Metadata = BasketMetadata() });

            _sessions.Verify(s => s.Clear("en-GB", "GBP", It.IsAny<CancellationToken>()), Times.Once);
        }

        private const string NoCheckoutSession = null;

        [Fact]
        public async Task ASessionWithoutBasketMetadataClearsItsCurrencyBasketOfTheRoutedPagesCulture()
        {
            // Checkout sessions created before the upgrade carry no basket metadata; their basket records no checkout session.
            CurrentBasketCheckedOutAs(NoCheckoutSession);

            await Succeed(new Session { Status = "complete", PaymentStatus = "paid", Currency = "gbp", Created = DateTime.UtcNow.AddMinutes(-10), Metadata = new Dictionary<string, string>() });

            _sessions.Verify(s => s.Clear("en-GB", "GBP", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ASessionWithoutBasketMetadataKeepsABasketSentToANewerCheckout()
        {
            CurrentBasketCheckedOutAs("cs_test_newer");

            await Succeed(new Session { Status = "complete", Currency = "gbp", Created = DateTime.UtcNow.AddMinutes(-10), Metadata = new Dictionary<string, string>() });

            _sessions.Verify(s => s.Clear(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task A_session_without_basket_metadata_clears_the_basket_that_records_it_whatever_its_age()
        {
            // The basket names this checkout session, so it is the paid one; the 24-hour rule is only for a basket that records none.
            CurrentBasketCheckedOutAs("cs_test_1");

            await Succeed(new Session { Status = "complete", Currency = "gbp", Created = DateTime.UtcNow.AddDays(-3), Metadata = new Dictionary<string, string>() });

            _sessions.Verify(s => s.Clear("en-GB", "GBP", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task A_session_without_basket_metadata_compares_the_recorded_id_ordinally()
        {
            CurrentBasketCheckedOutAs("CS_TEST_1");

            await Succeed(new Session { Status = "complete", Currency = "gbp", Created = DateTime.UtcNow.AddMinutes(-10), Metadata = new Dictionary<string, string>() });

            _sessions.Verify(s => s.Clear(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AnOldSessionWithoutBasketMetadataKeepsTheBasket()
        {
            // A stale link to a session from before the upgrade, revisited days later: the basket records no checkout session.
            CurrentBasketCheckedOutAs(NoCheckoutSession);

            await Succeed(new Session { Status = "complete", Currency = "gbp", Created = DateTime.UtcNow.AddDays(-3), Metadata = new Dictionary<string, string>() });

            _sessions.Verify(s => s.Clear(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task A_session_without_metadata_or_currency_keeps_the_basket()
        {
            await Succeed(new Session { Status = "complete", Created = DateTime.UtcNow.AddMinutes(-10), Metadata = new Dictionary<string, string>() });

            _sessions.Verify(s => s.Clear(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData("cs_test_other")] // a basket checked out again later, or a link someone else shared
        [InlineData(null)]            // a basket never sent to checkout, e.g. one started after paying
        public async Task A_basket_that_is_not_the_paid_one_is_kept(string? currentBasketCheckoutSessionId)
        {
            CurrentBasketCheckedOutAs(currentBasketCheckoutSessionId);

            await Succeed(new Session { Status = "complete", PaymentStatus = "paid", Currency = "gbp", Metadata = BasketMetadata() });

            _sessions.Verify(s => s.Clear(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
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
            _stripe.Setup(s => s.GetSessionAsync("cs_test_1", It.IsAny<CancellationToken>())).ThrowsAsync(new StripeException("No such checkout.session"));

            await Request("https://shop.test/success/?session_id=cs_test_1&success=true"); // no exception escapes

            _sessions.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task A_Stripe_timeout_is_logged_and_the_page_still_renders()
        {
            // Stripe.net rethrows an HttpClient timeout as a raw TaskCanceledException; the customer has just paid.
            _stripe.Setup(s => s.GetSessionAsync("cs_test_1", It.IsAny<CancellationToken>())).ThrowsAsync(new TaskCanceledException("The request was canceled due to the configured HttpClient.Timeout"));

            await Request("https://shop.test/success/?session_id=cs_test_1&success=true"); // no exception escapes

            _sessions.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task A_failure_while_matching_the_success_page_is_logged_and_the_page_still_renders()
        {
            var request = new Mock<IPublishedRequestBuilder>();
            request.Setup(r => r.Uri).Returns(new Uri("https://shop.test/success/?session_id=cs_test_1&success=true"));
            _matcher.Setup(m => m.IsSuccessPageAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("database down"));

            await new StripeResponseNotificationHandler(_stripe.Object, _sessions.Object, _matcher.Object, NullLogger<StripeResponseNotificationHandler>.Instance, _ => "en-GB")
                .HandleAsync(new RoutingRequestNotification(request.Object), CancellationToken.None); // no exception escapes

            _stripe.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task A_failure_clearing_the_basket_is_logged_and_the_page_still_renders()
        {
            // e.g. the session isn't available: the customer has paid and must still see the success page.
            _sessions.Setup(s => s.Clear(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Session has not been configured"));

            await Succeed(new Session { Status = "complete", PaymentStatus = "paid", Currency = "gbp", Metadata = BasketMetadata() }); // no exception escapes
        }

        [Theory]
        [InlineData("paid")]
        [InlineData("no_payment_required")]
        [InlineData("unpaid")] // delayed methods (Bacs, SEPA): checkout is committed; the bank's result arrives by webhook
        public async Task A_complete_session_clears_the_basket_whatever_its_payment_status(string paymentStatus)
        {
            await Succeed(new Session { Status = "complete", PaymentStatus = paymentStatus, Currency = "gbp", Metadata = BasketMetadata() });

            _sessions.Verify(s => s.Clear("en-GB", "GBP", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
