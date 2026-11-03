using UmbCheckout.Shared.Notifications;
using Umbraco.Cms.Core.Events;
using Xunit;

namespace UmbCheckout.Tests.Notifications;

public sealed class EventMessagesExtensionsTests
{
    [Fact]
    public void A_cancel_message_is_the_handlers_first_error_or_warning()
    {
        var messages = new EventMessages();
        messages.Add(new EventMessage("Info", "Just so you know", EventMessageType.Info));
        messages.Add(new EventMessage("Shipping", "Rates must start with shr_live_", EventMessageType.Error));
        messages.MarkCancelled();

        Assert.Equal("Rates must start with shr_live_", messages.CancellationMessage());
    }

    [Fact]
    public void A_cancel_without_a_message_still_reads_as_a_cancel()
    {
        // Umbraco's other documented form: notification.Cancel = true, with no message.
        var messages = new EventMessages();
        messages.MarkCancelled();

        Assert.Equal(EventMessagesExtensions.DefaultCancellationMessage, messages.CancellationMessage());
    }

    [Fact]
    public void A_warning_without_a_cancel_is_not_a_cancel()
    {
        var messages = new EventMessages();
        messages.Add(new EventMessage("Shipping", "This item ships late", EventMessageType.Warning));

        Assert.Null(messages.CancellationMessage());
        Assert.Null(((EventMessages?)null).CancellationMessage());
    }

    [Fact]
    public void A_cancel_read_once_is_cleared_so_a_later_failure_in_the_request_is_not_reported_as_cancelled()
    {
        // Review minor: a stale cancel in one request turned a later 404 into a 400.
        var messages = new EventMessages();
        messages.Add(new EventMessage("Rates", "Rates are locked", EventMessageType.Error));
        messages.MarkCancelled();

        Assert.Equal("Rates are locked", messages.TakeCancellationMessage());
        Assert.Null(messages.TakeCancellationMessage());
        Assert.Null(messages.CancellationMessage());

        // A new cancel after the read is reported with its own message, not the earlier one.
        messages.Add(new EventMessage("Rates", "Rates are frozen", EventMessageType.Warning));
        messages.MarkCancelled();
        Assert.Equal("Rates are frozen", messages.TakeCancellationMessage());
    }

    [Fact]
    public void Cancels_are_counted_so_one_operation_can_tell_if_it_was_cancelled()
    {
        var messages = new EventMessages();
        var before = messages.CancellationCount();
        messages.MarkCancelled();

        Assert.Equal(before + 1, messages.CancellationCount());
    }
}
