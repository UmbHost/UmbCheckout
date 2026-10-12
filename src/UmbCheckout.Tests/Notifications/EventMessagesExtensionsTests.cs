using UmbCheckout.Shared.Notifications;
using Umbraco.Cms.Core.Events;
using Xunit;

namespace UmbCheckout.Tests.Notifications;

public sealed class EventMessagesExtensionsTests
{
    [Fact]
    public void A_cancel_message_is_the_first_error_or_warning()
    {
        var messages = new EventMessages();
        messages.Add(new EventMessage("Info", "Just so you know", EventMessageType.Info));
        messages.Add(new EventMessage("Shipping", "Rates must start with shr_live_", EventMessageType.Error));

        Assert.Equal("Rates must start with shr_live_", messages.CancellationMessage());
    }

    [Fact]
    public void No_error_or_warning_means_no_cancel()
    {
        var messages = new EventMessages();
        messages.Add(new EventMessage("Info", "Saved", EventMessageType.Success));

        Assert.Null(messages.CancellationMessage());
        Assert.Null(((EventMessages?)null).CancellationMessage());
    }
}
