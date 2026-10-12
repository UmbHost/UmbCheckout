using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UmbCheckout.Stripe.Controllers.Api;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Notifications.Webhooks;
using Umbraco.Cms.Core.Events;
using Xunit;

namespace UmbCheckout.Tests.Api
{
    /// <summary>
    /// The Stripe webhook: signed events publish their notification once; redeliveries are acknowledged without handlers;
    /// failures answer so Stripe retries (500) or stops (400 for a bad signature).
    /// </summary>
    public class StripeWebhookApiControllerTests
    {
        private const string Secret = "whsec_test_secret";
        private readonly Mock<IEventAggregator> _events = new();
        private readonly Mock<IStripeWebhookEventStore> _store = new();

        public StripeWebhookApiControllerTests()
            => _store.Setup(s => s.TryClaimAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(StripeWebhookEventClaim.Claimed);

        private static string Json(string id, string type, string apiVersion = "2026-01-28.clover") =>
            // Real Stripe events always carry "request"; Stripe.net's event converter relies on it.
            "{\"id\":\"" + id + "\",\"object\":\"event\",\"api_version\":\"" + apiVersion + "\",\"created\":1760000000,\"livemode\":false,\"pending_webhooks\":1,"
            + "\"request\":{\"id\":null,\"idempotency_key\":null},\"type\":\"" + type
            + "\",\"data\":{\"object\":{\"id\":\"cs_test_1\",\"object\":\"checkout.session\"}}}";

        private static string Sign(string json, string secret)
        {
            var t = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var signature = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{t}.{json}"))).ToLowerInvariant();
            return $"t={t},v1={signature}";
        }

        private Task<IActionResult> PostAsync(string json, string? signature = null, string secret = Secret)
        {
            var secrets = new Mock<IStripeWebhookSecretProvider>();
            secrets.Setup(p => p.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync((secret, false));
            var controller = new StripeWebhookApiController(NullLogger<StripeWebhookApiController>.Instance, _events.Object, secrets.Object, _store.Object);
            var context = new DefaultHttpContext();
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
            context.Request.Headers["Stripe-Signature"] = signature ?? Sign(json, Secret);
            controller.ControllerContext = new ControllerContext { HttpContext = context };
            return controller.CheckoutEvents();
        }

        [Fact]
        public async Task A_signed_event_publishes_its_notification()
        {
            var result = await PostAsync(Json("evt_1", "checkout.session.completed"));

            Assert.IsType<AcceptedResult>(result);
            _events.Verify(e => e.PublishAsync(It.IsAny<OnCheckoutSessionCompletedNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task A_bad_signature_is_a_400_and_publishes_nothing()
        {
            // Signed over different content, so the header doesn't match the body.
            var result = await PostAsync(Json("evt_1", "checkout.session.completed"), signature: Sign("tampered", Secret));

            Assert.IsType<BadRequestResult>(result);
            _events.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task A_redelivered_event_is_acknowledged_without_running_handlers()
        {
            _store.Setup(s => s.TryClaimAsync("evt_1", It.IsAny<CancellationToken>())).ReturnsAsync(StripeWebhookEventClaim.AlreadyHandled);

            Assert.IsType<OkResult>(await PostAsync(Json("evt_1", "checkout.session.completed")));
            _events.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task A_failing_handler_returns_500_and_the_retry_runs_it()
        {
            _events.Setup(e => e.PublishAsync(It.IsAny<OnCheckoutSessionCompletedNotification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("fulfilment down"));

            var result = Assert.IsType<StatusCodeResult>(await PostAsync(Json("evt_1", "checkout.session.completed")));

            Assert.Equal(500, result.StatusCode);
            _store.Verify(s => s.ForgetAsync("evt_1", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task A_handled_event_is_marked_done()
        {
            await PostAsync(Json("evt_1", "checkout.session.completed"));

            _store.Verify(s => s.CompleteAsync("evt_1", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task A_redelivery_while_the_first_is_running_is_asked_to_retry_later()
        {
            _store.Setup(s => s.TryClaimAsync("evt_1", It.IsAny<CancellationToken>())).ReturnsAsync(StripeWebhookEventClaim.InProgress);

            var result = Assert.IsType<StatusCodeResult>(await PostAsync(Json("evt_1", "checkout.session.completed")));

            Assert.Equal(409, result.StatusCode);
            _events.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task A_failing_release_still_answers_500_so_Stripe_retries()
        {
            // The handler failed because the database is down, so releasing the claim fails too; the lease expires instead.
            _events.Setup(e => e.PublishAsync(It.IsAny<OnCheckoutSessionCompletedNotification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("database down"));
            _store.Setup(s => s.ForgetAsync("evt_1", It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("database down"));

            var result = Assert.IsType<StatusCodeResult>(await PostAsync(Json("evt_1", "checkout.session.completed")));

            Assert.Equal(500, result.StatusCode);
            _store.Verify(s => s.CompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task An_unhandled_type_is_not_recorded()
        {
            await PostAsync(Json("evt_3", "customer.created"));

            _store.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task A_missing_webhook_secret_is_logged_and_returns_500()
        {
            var result = Assert.IsType<StatusCodeResult>(await PostAsync(Json("evt_1", "checkout.session.completed"), secret: ""));

            Assert.Equal(500, result.StatusCode);
            _events.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task An_event_on_another_API_version_is_still_handled()
        {
            // An endpoint created on another API version (a new account may not be able to pick ours) still works.
            var result = await PostAsync(Json("evt_2", "checkout.session.completed", apiVersion: "2024-06-20"));

            Assert.IsType<AcceptedResult>(result);
            _events.Verify(e => e.PublishAsync(It.IsAny<OnCheckoutSessionCompletedNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task An_unhandled_type_is_accepted()
            => Assert.IsType<AcceptedResult>(await PostAsync(Json("evt_3", "customer.created")));
    }
}
