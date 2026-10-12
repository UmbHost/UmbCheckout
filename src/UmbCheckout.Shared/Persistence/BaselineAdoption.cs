using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UmbCheckout.Shared.Persistence;

/// <summary>Marks a context's Baseline migration as applied on a database whose tables already exist (created by NPoco before v17).</summary>
public static class BaselineAdoption
{
    public const string BaselineName = "Baseline";

    public static async Task<bool> AdoptAsync(DbContext db, CancellationToken cancellationToken = default)
    {
        var baselineId = db.GetService<IMigrationsAssembly>().FindMigrationId(BaselineName)
            ?? throw new InvalidOperationException($"{db.GetType().Name} has no {BaselineName} migration for {db.Database.ProviderName}.");
        var history = db.GetService<IHistoryRepository>();

        await history.CreateIfNotExistsAsync(cancellationToken);
        var applied = await history.GetAppliedMigrationsAsync(cancellationToken);
        if (applied.Any(row => row.MigrationId == baselineId))
        {
            return false;
        }

        await db.Database.ExecuteSqlRawAsync(history.GetInsertScript(new HistoryRow(baselineId, ProductInfo.GetVersion())), cancellationToken);
        return true;
    }
}
