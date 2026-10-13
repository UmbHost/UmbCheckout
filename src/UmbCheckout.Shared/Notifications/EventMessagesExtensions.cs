using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications
{
    public static class EventMessagesExtensions
    {
        /// <summary>The message shown when a handler cancelled without giving one (<c>notification.Cancel = true</c>).</summary>
        public const string DefaultCancellationMessage = "A rule on this site stopped this change.";

        /// <summary>The category of the marker UmbCheckout adds when a handler cancels; not a message for people.</summary>
        public const string CancelledCategory = "UmbCheckout.Cancelled";

        /// <summary>Records that a notification handler cancelled an operation in this request.</summary>
        public static void MarkCancelled(this EventMessages messages)
            => messages.Add(new EventMessage(CancelledCategory, string.Empty, EventMessageType.Info));

        /// <summary>How many operations handlers have cancelled in this request; compare before and after one operation.</summary>
        public static int CancellationCount(this EventMessages? messages)
            => messages?.GetAll().Count(m => m.Category == CancelledCategory) ?? 0;

        /// <summary>The category of the marker <see cref="TakeCancellationMessage"/> adds: cancels before it have been reported.</summary>
        public const string CancellationReadCategory = "UmbCheckout.CancellationRead";

        /// <summary>
        /// When a handler cancelled an operation in this request (since the last <see cref="TakeCancellationMessage"/>), the message for the
        /// person who tried: the handler's first Error or Warning message, or <see cref="DefaultCancellationMessage"/>. Null when nothing
        /// was cancelled (a Warning on its own is not a cancel).
        /// </summary>
        public static string? CancellationMessage(this EventMessages? messages)
        {
            if (messages is null)
            {
                return null;
            }

            // Umbraco's EventMessages can't remove a message: a read marker hides everything before it.
            var all = messages.GetAll().ToList();
            var unread = all.Skip(all.FindLastIndex(m => m.Category == CancellationReadCategory) + 1).ToList();
            return unread.Any(m => m.Category == CancelledCategory)
                ? unread.FirstOrDefault(m => m.Category is not (CancelledCategory or CancellationReadCategory) && m.MessageType is EventMessageType.Error or EventMessageType.Warning)?.Message
                  ?? DefaultCancellationMessage
                : null;
        }

        /// <summary>As <see cref="CancellationMessage"/>, and clears the cancel once read, so a later failure in the same request isn't reported as cancelled.</summary>
        public static string? TakeCancellationMessage(this EventMessages? messages)
        {
            var message = messages.CancellationMessage();
            if (message is not null)
            {
                messages!.Add(new EventMessage(CancellationReadCategory, string.Empty, EventMessageType.Info));
            }

            return message;
        }

        /// <summary>
        /// Publishes a before-save or before-delete notification and records a cancel. Skipped (not cancelled) inside
        /// <see cref="UmbCheckoutHooks.SuppressSaving"/>.
        /// </summary>
        /// <returns>True when a handler cancelled.</returns>
        public static async Task<bool> PublishCancellingAsync<TNotification>(this IEventAggregator eventAggregator, TNotification notification, EventMessages messages)
            where TNotification : ICancelableNotification
        {
            if (UmbCheckoutHooks.SavingSuppressed || !await eventAggregator.PublishCancelableAsync(notification))
            {
                return false;
            }

            messages.MarkCancelled();
            return true;
        }
    }
}
