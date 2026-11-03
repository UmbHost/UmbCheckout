using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Testing;

/// <summary>An <see cref="IEventAggregator"/> that records notifications and runs test handlers immediately.</summary>
public sealed class TestEventAggregator : IEventAggregator
{
    private readonly List<Func<INotification, Task>> _handlers = [];

    public List<INotification> Published { get; } = [];

    public TestEventAggregator On<T>(Action<T> handler) where T : INotification
    {
        _handlers.Add(n =>
        {
            if (n is T typed)
            {
                handler(typed);
            }

            return Task.CompletedTask;
        });
        return this;
    }

    public Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        Published.Add(notification);
        return RunAsync(notification);
    }

    public async Task PublishAsync<TNotification, TNotificationHandler>(IEnumerable<TNotification> notifications, CancellationToken cancellationToken = default)
        where TNotification : INotification where TNotificationHandler : INotificationHandler
    {
        foreach (var notification in notifications)
        {
            await PublishAsync(notification, cancellationToken);
        }
    }

    public void Publish<TNotification>(TNotification notification) where TNotification : INotification => PublishAsync(notification).GetAwaiter().GetResult();

    public void Publish<TNotification, TNotificationHandler>(IEnumerable<TNotification> notifications)
        where TNotification : INotification where TNotificationHandler : INotificationHandler
        => PublishAsync<TNotification, TNotificationHandler>(notifications).GetAwaiter().GetResult();

    public async Task<bool> PublishCancelableAsync<TCancelableNotification>(TCancelableNotification notification) where TCancelableNotification : ICancelableNotification
    {
        Published.Add(notification);
        await RunAsync(notification);
        return notification.Cancel;
    }

    public bool PublishCancelable<TCancelableNotification>(TCancelableNotification notification) where TCancelableNotification : ICancelableNotification
        => PublishCancelableAsync(notification).GetAwaiter().GetResult();

    private async Task RunAsync(INotification notification)
    {
        foreach (var handler in _handlers)
        {
            await handler(notification);
        }
    }
}
