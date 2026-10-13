using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UmbCheckout.Stripe.Controllers.Api;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Notifications.Webhooks;
using UmbCheckout.Tests.Helpers;
using Umbraco.Cms.Core.Events;
using Xunit;

namespace UmbCheckout.Tests.Controllers
{
    /// <summary>
    /// The Stripe webhook: each event is handled once (a redelivery is acknowledged without running handlers), a failed handler
    /// answers 500 so Stripe retries, a missing secret is logged, and events on other API versions are accepted with a warning.
    /// </summary>
    public class StripeWebhookApiControllerTests
    {
        // A made-up signing secret for these tests only.
        private const string Secret = "whsec_unit_test_only";
        private readonly Mock<IEventAggregator> _events = new();
        private readonly Mock<IStripeWebhookEventStore> _store = new();
        private readonly RecordingLogger<StripeWebhookApiController> _logger = new();

        public StripeWebhookApiControllerTests()
        {
            _store.Setup(s => s.TryClaimAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(StripeWebhookEventClaim.Claimed);
        }

        private static string Json(string id, string type, string apiVersion = "2026-01-28.clover") =>
            "{\"id\":\"" + id + "\",\"object\":\"event\",\"api_version\":\"" + apiVersion + "\",\"created\":1760000000,\"livemode\":false,\"type\":\"" + type
            + "\",\"pending_webhooks\":1,\"request\":{\"id\":null,\"idempotency_key\":null},\"data\":{\"object\":{\"id\":\"cs_test_1\",\"object\":\"checkout.session\"}}}";

        private static string Sign(string json, string secret)
        {
            var t = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var signature = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{t}.{json}"))).Replace("-", string.Empty).ToLowerInvariant();
            return $"t={t},v1={signature}";
        }

        private async Task<IActionResult> PostAsync(string json, string? signature = null, string secret = Secret)
        {
            var provider = new Mock<IStripeWebhookSecretProvider>();
            provider.Setup(p => p.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync((secret, false));
            var controller = new StripeWebhookApiController(_logger, _events.Object, provider.Object, _store.Object);
            var context = new DefaultHttpContext();
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
            context.Request.Headers["Stripe-Signature"] = signature ?? Sign(json, Secret);
            controller.ControllerContext = new ControllerContext { HttpContext = context };
            return await controller.CheckoutEvents();
        }

        [Fact]
        public async Task A_signed_event_publishes_its_notification_and_is_marked_done()
        {
            var result = await PostAsync(Json("evt_1", "checkout.session.completed"));

            Assert.IsType<AcceptedResult>(result);
            _events.Verify(e => e.PublishAsync(It.IsAny<OnCheckoutSessionCompletedNotification>(), It.IsAny<CancellationToken>()), Times.Once);
            _store.Verify(s => s.CompleteAsync("evt_1", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task A_bad_signature_is_a_400_and_publishes_nothing()
        {
            var result = await PostAsync(Json("evt_1", "checkout.session.completed"), signature: Sign("tampered", Secret));

            Assert.IsType<BadRequestResult>(result);
            _events.VerifyNoOtherCalls();
            _store.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task A_redelivered_event_is_acknowledged_without_running_handlers()
        {
            _store.Setup(s => s.TryClaimAsync("evt_1", It.IsAny<CancellationToken>())).ReturnsAsync(StripeWebhookEventClaim.AlreadyHandled);

            Assert.IsType<OkResult>(await PostAsync(Json("evt_1", "checkout.session.completed")));
            _events.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task An_event_another_delivery_is_running_is_a_conflict_so_Stripe_retries()
        {
            _store.Setup(s => s.TryClaimAsync("evt_1", It.IsAny<CancellationToken>())).ReturnsAsync(StripeWebhookEventClaim.InProgress);

            Assert.Equal(409, Assert.IsType<StatusCodeResult>(await PostAsync(Json("evt_1", "checkout.session.completed"))).StatusCode);
            _events.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task A_failing_handler_returns_500_and_the_retry_runs_it()
        {
            _events.Setup(e => e.PublishAsync(It.IsAny<OnCheckoutSessionCompletedNotification>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("fulfilment down"));

            var result = Assert.IsType<StatusCodeResult>(await PostAsync(Json("evt_1", "checkout.session.completed")));

            Assert.Equal(500, result.StatusCode);
            _store.Verify(s => s.ForgetAsync("evt_1", It.IsAny<CancellationToken>()), Times.Once);
            _store.Verify(s => s.CompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            Assert.Equal(1, _logger.Count(LogLevel.Error));
        }

        [Fact]
        public async Task Handlers_run_to_the_end_even_if_Stripe_drops_the_connection()
        {
            await PostAsync(Json("evt_1", "checkout.session.completed"));

            _events.Verify(e => e.PublishAsync(It.IsAny<OnCheckoutSessionCompletedNotification>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task A_missing_webhook_secret_is_logged_and_returns_500()
        {
            var result = Assert.IsType<StatusCodeResult>(await PostAsync(Json("evt_1", "checkout.session.completed"), secret: ""));

            Assert.Equal(500, result.StatusCode);
            _events.VerifyNoOtherCalls();
            var (level, message) = Assert.Single(_logger.Entries);
            Assert.Equal(LogLevel.Error, level);
            Assert.Contains("test", message);
        }

        [Fact]
        public async Task An_event_on_another_API_version_is_still_handled_with_a_warning()
        {
            var result = await PostAsync(Json("evt_2", "checkout.session.completed", apiVersion: "2024-06-20.unit-test"));

            Assert.IsType<AcceptedResult>(result);
            _events.Verify(e => e.PublishAsync(It.IsAny<OnCheckoutSessionCompletedNotification>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.Contains(_logger.Entries, entry => entry.Level == LogLevel.Warning && entry.Message.Contains("2024-06-20.unit-test"));
        }

        [Fact]
        public async Task An_unhandled_type_is_accepted_without_a_claim()
        {
            Assert.IsType<AcceptedResult>(await PostAsync(Json("evt_3", "customer.created")));
            _store.VerifyNoOtherCalls();
        }

        [Fact]
        public void The_old_constructor_is_kept_and_obsolete_and_DI_picks_the_new_one()
        {
            var constructors = typeof(StripeWebhookApiController).GetConstructors();

            Assert.Equal(2, constructors.Length);
            Assert.Single(constructors, c => c.GetCustomAttributes(typeof(ObsoleteAttribute), false).Any());
            Assert.Single(constructors, c => c.GetCustomAttributes(typeof(Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructorAttribute), false).Any()
                                             && c.GetParameters().Any(p => p.ParameterType == typeof(IStripeWebhookEventStore)));
        }
    }
}
