using Microsoft.Extensions.Logging;
using Moq;
using StripeException = Stripe.StripeException;
using Stripe.Checkout;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.NotificationHandlers;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Tests.Helpers;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Routing;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    /// <summary>
    /// After payment the paid basket is cleared, on the configured success page only, and only the basket the returning checkout
    /// session paid for. The handler runs while routing, before the basket context knows the page, so the basket's culture and
    /// currency travel in the Stripe session's metadata.
    /// </summary>
    public class StripeResponseNotificationHandlerTests
    {
        private readonly Mock<IStripeSessionService> _stripe = new();
        private readonly Mock<ISessionService> _sessions = new();
        private readonly Mock<IUmbCheckoutSuccessPageMatcher> _matcher = new();
        private readonly RecordingLogger<StripeResponseNotificationHandler> _logger = new();
        private readonly IPublishedContent _page = Mock.Of<IPublishedContent>();

        private void Basket(string? culture, string currency, string? checkoutSessionId)
            => _sessions.Setup(s => s.Get(culture, currency)).ReturnsAsync(new UmbCheckoutSession
            {
                Basket = new Basket { SiteRootCulture = culture, CurrencyCode = currency, CheckoutSessionId = checkoutSessionId, LineItems = new[] { new LineItem() } }
            });

        private Task Request(string url, bool isSuccessPage = true, string? routedCulture = "en-GB")
        {
            _matcher.Setup(m => m.IsSuccessPageAsync(_page, It.IsAny<Uri>(), It.IsAny<CancellationToken>())).ReturnsAsync(isSuccessPage);
            var request = new Mock<IPublishedRequestBuilder>();
            request.Setup(r => r.Uri).Returns(new Uri(url));
            request.Setup(r => r.PublishedContent).Returns(_page);
            return new StripeResponseNotificationHandler(_stripe.Object, _sessions.Object, _matcher.Object, _logger, _ => routedCulture)
                .HandleAsync(new RoutingRequestNotification(request.Object), CancellationToken.None);
        }

        private Task Succeed(Session session, string? routedCulture = "en-GB")
        {
            session.Id ??= "cs_test_1";
            _stripe.Setup(s => s.GetSessionAsync("cs_test_1")).ReturnsAsync(session);
            return Request("https://shop.test/success/?session_id=cs_test_1&success=true", routedCulture: routedCulture);
        }

        private static Dictionary<string, string> Metadata(string culture, string currency)
            => new() { ["umbcheckout_culture"] = culture, ["umbcheckout_currency"] = currency };

        [Fact]
        public async Task ClearsThePaidBasketsCultureAndCurrency()
        {
            Basket("en-GB", "GBP", "cs_test_1");

            await Succeed(new Session { Status = "complete", PaymentStatus = "paid", Currency = "gbp", Metadata = Metadata("en-GB", "GBP") });

            _sessions.Verify(s => s.Clear("en-GB", "GBP"), Times.Once);
            _sessions.Verify(s => s.Clear(), Times.Never);
        }

        [Fact]
        public async Task ABasketWithNoCultureIsClearedAsSuch()
        {
            // A site without domains: the basket's culture is null (stored as an empty value).
            Basket(null, "USD", "cs_test_1");

            await Succeed(new Session { Status = "complete", PaymentStatus = "paid", Currency = "usd", Metadata = Metadata("", "USD") });

            _sessions.Verify(s => s.Clear(null, "USD"), Times.Once);
        }

        [Theory]
        [InlineData(null)] // a basket started after paying (or never sent to checkout)
        [InlineData("cs_test_other")] // a basket sent to a later checkout
        public async Task A_basket_that_is_not_the_paid_one_is_kept(string? checkoutSessionId)
        {
            // Revisiting an old or shared success link must not clear the visitor's current basket.
            Basket("en-GB", "GBP", checkoutSessionId);

            await Succeed(new Session { Status = "complete", PaymentStatus = "paid", Currency = "gbp", Metadata = Metadata("en-GB", "GBP") });

            _sessions.Verify(s => s.Clear(It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task ASessionWithoutBasketMetadataClearsItsCurrencyBasketOfTheRoutedPagesCulture()
        {
            // Checkout sessions created before the upgrade carry no basket metadata; their basket records no checkout session.
            Basket("en-GB", "GBP", null);

            await Succeed(new Session { Status = "complete", PaymentStatus = "paid", Currency = "gbp", Created = DateTime.UtcNow.AddMinutes(-10), Metadata = new Dictionary<string, string>() });

            _sessions.Verify(s => s.Clear("en-GB", "GBP"), Times.Once);
        }

        [Fact]
        public async Task ASessionWithoutBasketMetadataKeepsABasketSentToANewerCheckout()
        {
            Basket("en-GB", "GBP", "cs_test_newer");

            await Succeed(new Session { Status = "complete", Currency = "gbp", Created = DateTime.UtcNow.AddMinutes(-10), Metadata = new Dictionary<string, string>() });

            _sessions.Verify(s => s.Clear(It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task A_session_without_basket_metadata_clears_the_basket_that_records_it_whatever_its_age()
        {
            // Group 4 review M1: the basket names this checkout session, so it is the paid one; the 24-hour rule is only for a
            // basket that records no checkout session.
            Basket("en-GB", "GBP", "cs_test_1");

            await Succeed(new Session { Status = "complete", Currency = "gbp", Created = DateTime.UtcNow.AddDays(-3), Metadata = new Dictionary<string, string>() });

            _sessions.Verify(s => s.Clear("en-GB", "GBP"), Times.Once);
        }

        [Fact]
        public async Task A_session_without_basket_metadata_compares_the_recorded_id_ordinally()
        {
            Basket("en-GB", "GBP", "CS_TEST_1");

            await Succeed(new Session { Status = "complete", Currency = "gbp", Created = DateTime.UtcNow.AddMinutes(-10), Metadata = new Dictionary<string, string>() });

            _sessions.Verify(s => s.Clear(It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task AnOldSessionWithoutBasketMetadataKeepsTheBasket()
        {
            // A stale link to a session from before the upgrade, revisited days later.
            Basket("en-GB", "GBP", null);

            await Succeed(new Session { Status = "complete", Currency = "gbp", Created = DateTime.UtcNow.AddDays(-3), Metadata = new Dictionary<string, string>() });

            _sessions.Verify(s => s.Clear(It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task AnIncompleteSessionClearsNothing()
        {
            await Succeed(new Session { Status = "open", Currency = "gbp", Metadata = new Dictionary<string, string>() });

            _sessions.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("paid")]
        [InlineData("no_payment_required")]
        [InlineData("unpaid")] // delayed methods (Bacs, SEPA): checkout is committed; the result arrives by webhook
        public async Task A_complete_session_clears_the_basket_whatever_its_payment_status(string paymentStatus)
        {
            Basket("en-GB", "GBP", "cs_test_1");

            await Succeed(new Session { Status = "complete", PaymentStatus = paymentStatus, Currency = "gbp", Metadata = Metadata("en-GB", "GBP") });

            _sessions.Verify(s => s.Clear("en-GB", "GBP"), Times.Once);
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
        [InlineData("cs_test_1&other=x' OR 1=1")]
        [InlineData("cs_test_../../v1/customers")]
        public async Task A_session_id_that_is_not_a_checkout_session_is_ignored(string sessionId)
        {
            await Request($"https://shop.test/success/?session_id={Uri.EscapeDataString(sessionId)}&success=true");

            _stripe.VerifyNoOtherCalls();
            _sessions.VerifyNoOtherCalls();
            Assert.Equal(1, _logger.Count(LogLevel.Warning));
        }

        [Fact]
        public async Task A_Stripe_error_is_logged_and_the_page_still_renders()
        {
            _stripe.Setup(s => s.GetSessionAsync("cs_test_1")).ThrowsAsync(new StripeException("No such checkout.session"));

            await Request("https://shop.test/success/?session_id=cs_test_1&success=true"); // no exception escapes

            _sessions.VerifyNoOtherCalls();
            Assert.Equal(1, _logger.Count(LogLevel.Warning));
        }

        [Fact]
        public async Task A_timeout_reaching_Stripe_is_logged_and_the_page_still_renders()
        {
            _stripe.Setup(s => s.GetSessionAsync("cs_test_1")).ThrowsAsync(new TaskCanceledException("HttpClient timeout"));

            await Request("https://shop.test/success/?session_id=cs_test_1&success=true");

            _sessions.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task A_failure_while_matching_the_success_page_is_logged_and_the_page_still_renders()
        {
            _matcher.Setup(m => m.IsSuccessPageAsync(It.IsAny<IPublishedContent?>(), It.IsAny<Uri>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("database down"));
            var request = new Mock<IPublishedRequestBuilder>();
            request.Setup(r => r.Uri).Returns(new Uri("https://shop.test/success/?session_id=cs_test_1&success=true"));

            await new StripeResponseNotificationHandler(_stripe.Object, _sessions.Object, _matcher.Object, _logger, _ => null)
                .HandleAsync(new RoutingRequestNotification(request.Object), CancellationToken.None);

            _stripe.VerifyNoOtherCalls();
            Assert.Equal(1, _logger.Count(LogLevel.Warning));
        }

        [Fact]
        public async Task A_failure_clearing_the_basket_is_logged_and_the_page_still_renders()
        {
            Basket("en-GB", "GBP", "cs_test_1");
            _sessions.Setup(s => s.Clear("en-GB", "GBP")).ThrowsAsync(new InvalidOperationException("Session has not been configured"));

            await Succeed(new Session { Status = "complete", PaymentStatus = "paid", Currency = "gbp", Metadata = Metadata("en-GB", "GBP") });

            Assert.Equal(1, _logger.Count(LogLevel.Error));
        }

        [Fact]
        public void The_old_constructor_is_kept_and_obsolete_and_DI_picks_the_new_one()
        {
            var constructors = typeof(StripeResponseNotificationHandler).GetConstructors();

            Assert.Equal(2, constructors.Length);
            Assert.Single(constructors, c => c.GetCustomAttributes(typeof(ObsoleteAttribute), false).Any());
            // Umbraco's container picks the constructor with the most parameters it can satisfy.
            Assert.Contains(typeof(IUmbCheckoutSuccessPageMatcher), constructors.OrderByDescending(c => c.GetParameters().Length).First().GetParameters().Select(p => p.ParameterType));
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
