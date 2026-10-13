using Microsoft.EntityFrameworkCore;
using UmbCheckout.Testing;
using UmbCheckout.Tests.Persistence.Probe;
using Umbraco.Cms.Core.Notifications;
using Xunit;

namespace UmbCheckout.Tests.Persistence;

[Trait("Category", "Integration")] // SQL Server cases need Docker
[Collection(DatabaseCollection.Name)]
public sealed class TestEfCoreScopeTests(SqlServerContainerFixture sqlServer)
{
    private sealed record Saved : INotification;

    [Fact]
    public async Task A_scope_that_is_not_completed_rolls_back_and_drops_its_notifications()
    {
        await using var database = await TestDatabase.CreateAsync<ProbeDbContext>(TestProvider.Sqlite, sqlServer, o => new ProbeDbContext(o), ProbeDbContext.HistoryTable);
        await using (var db = database.CreateContext())
        {
            await db.Database.MigrateAsync();
        }

        using (var scope = database.ScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async db =>
            {
                db.Rows.Add(new ProbeRow { Name = "x", Amount = 1m });
                await db.SaveChangesAsync();
            });
            scope.Notifications.Publish(new Saved());
        }

        Assert.Equal(0, await database.ScalarAsync<int>("SELECT COUNT(*) FROM ProbeRows"));
        Assert.Empty(database.ScopeProvider.Notifications.Published);
    }
}
