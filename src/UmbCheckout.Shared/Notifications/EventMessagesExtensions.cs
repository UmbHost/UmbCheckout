using Umbraco.Cms.Core.Events;

namespace UmbCheckout.Shared.Notifications
{
    public static class EventMessagesExtensions
    {
        /// <summary>
        /// The message a notification handler gave when it cancelled an operation in this request: the first Error or Warning
        /// event message. Handlers cancel with <c>CancelOperation(new EventMessage(category, message, EventMessageType.Error))</c>.
        /// </summary>
        public static string? CancellationMessage(this EventMessages? messages)
            => messages?.GetAll().FirstOrDefault(m => m.MessageType is EventMessageType.Error or EventMessageType.Warning)?.Message;
    }
}
