using Moq;
using Stripe;
using Stripe.Checkout;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.NotificationHandlers;
using UmbCheckout.Stripe.Notifications.Webhooks;
using Xunit;

namespace UmbCheckout.Tests.NotificationHandlers;

/// <summary>
/// Sub-project B: the webhook records a checkout session as paid, so the customer's next basket read clears the paid basket
/// even if they never return to the success page. Delayed payment methods are recorded when the payment succeeds.
/// </summary>
public sealed class RecordPaidCheckoutSessionHandlerTests
{
    private readonly Mock<IStripePaidCheckoutSessions> _paid = new();

    private RecordPaidCheckoutSessionHandler Handler() => new(_paid.Object);

    private static Event SessionEvent(string type, string paymentStatus, string id = "cs_test_1")
        => new() { Id = "evt_1", Type = type, Data = new EventData { Object = new Session { Id = id, PaymentStatus = paymentStatus } } };

    [Theory]
    [InlineData("paid")]
    [InlineData("no_payment_required")]
    public async Task A_completed_and_paid_session_is_recorded(string paymentStatus)
    {
        await Handler().HandleAsync(new OnCheckoutSessionCompletedNotification(SessionEvent("checkout.session.completed", paymentStatus)), CancellationToken.None);

        _paid.Verify(p => p.MarkPaidAsync("cs_test_1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task A_completed_session_still_awaiting_a_delayed_payment_is_not_recorded()
    {
        await Handler().HandleAsync(new OnCheckoutSessionCompletedNotification(SessionEvent("checkout.session.completed", "unpaid")), CancellationToken.None);

        _paid.Verify(p => p.MarkPaidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task A_delayed_payment_that_succeeds_is_recorded()
    {
        await Handler().HandleAsync(new OnPaymentSuccessNotification(SessionEvent("checkout.session.async_payment_succeeded", "paid")), CancellationToken.None);

        _paid.Verify(p => p.MarkPaidAsync("cs_test_1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task An_event_without_a_checkout_session_is_ignored()
    {
        await Handler().HandleAsync(new OnCheckoutSessionCompletedNotification(new Event { Id = "evt_2", Type = "checkout.session.completed" }), CancellationToken.None);

        _paid.Verify(p => p.MarkPaidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task A_store_failure_propagates_so_Stripe_retries()
    {
        _paid.Setup(p => p.MarkPaidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("database down"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Handler().HandleAsync(new OnCheckoutSessionCompletedNotification(SessionEvent("checkout.session.completed", "paid")), CancellationToken.None));
    }
}
