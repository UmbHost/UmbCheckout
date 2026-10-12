using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Testing;

/// <summary>Holds notifications until the outermost scope exits, like Umbraco's scoped publisher: delivered only if it completed.</summary>
public sealed class RecordingNotificationPublisher : IScopedNotificationPublisher
{
    private readonly List<INotification> _pending = [];
    private readonly List<INotification> _published = [];

    public IReadOnlyList<INotification> Published => _published;

    public void Publish(INotification notification) => _pending.Add(notification);

    public bool PublishCancelable(ICancelableNotification notification)
    {
        _pending.Add(notification);
        return false;
    }

    public Task<bool> PublishCancelableAsync(ICancelableNotification notification) => Task.FromResult(PublishCancelable(notification));

    public void ScopeExit(bool completed)
    {
        if (completed)
        {
            _published.AddRange(_pending);
        }

        _pending.Clear();
    }

    public IDisposable Suppress() => new NoopDisposable();

    private sealed class NoopDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
