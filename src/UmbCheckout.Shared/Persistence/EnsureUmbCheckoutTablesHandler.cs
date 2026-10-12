using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace UmbCheckout.Shared.Persistence;

public static class EnsureUmbCheckoutTablesBuilderExtensions
{
    /// <summary>
    /// Migrates the UmbCheckout tables when <typeparamref name="TNotification"/> is raised. Several packages may ask for the same
    /// notification; the handler is added once.
    /// </summary>
    public static IUmbracoBuilder EnsureUmbCheckoutTablesOn<TNotification>(this IUmbracoBuilder builder)
        where TNotification : INotification
    {
        if (!builder.Services.Any(d => d.ImplementationType == typeof(EnsureUmbCheckoutTablesHandler<TNotification>)))
        {
            builder.AddNotificationAsyncHandler<TNotification, EnsureUmbCheckoutTablesHandler<TNotification>>();
        }

        return builder;
    }
}

/// <summary>
/// Makes sure the UmbCheckout tables exist before something that needs them runs ahead of startup. On a fresh install Umbraco runs package
/// migrations (uSync's first-boot import among them) before it raises <see cref="UmbracoApplicationStartingNotification"/>, where the tables
/// are normally migrated; the uSync packages register this for uSync's import-starting notification.
/// </summary>
public sealed class EnsureUmbCheckoutTablesHandler<TNotification>(IEnumerable<IUmbCheckoutTablesMigrator> migrators)
    : INotificationAsyncHandler<TNotification>
    where TNotification : INotification
{
    public async Task HandleAsync(TNotification notification, CancellationToken cancellationToken)
    {
        foreach (var migrator in migrators)
        {
            await migrator.MigrateAsync(cancellationToken);
        }
    }
}
