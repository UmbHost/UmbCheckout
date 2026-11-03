namespace UmbCheckout.Shared.Persistence;

/// <summary>Brings one set of UmbCheckout tables up to date. Idempotent; every <see cref="EfCoreDatabaseMigrator{TContext}"/> is one.</summary>
public interface IUmbCheckoutTablesMigrator
{
    Task MigrateAsync(CancellationToken cancellationToken = default);
}
