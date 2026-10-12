using Microsoft.EntityFrameworkCore;
using UmbCheckout.Shared.Persistence;
using UmbCheckout.Testing;
using UmbCheckout.Tests.Persistence.Probe;
using Xunit;

namespace UmbCheckout.Tests.Persistence;

[Collection(DatabaseCollection.Name)]
public sealed class ProviderMigrationsTests(SqlServerContainerFixture sqlServer)
{
    public static TheoryData<TestProvider> Providers => new() { TestProvider.Sqlite, TestProvider.SqlServer };

    private Task<TestDatabase<ProbeDbContext>> NewDatabase(TestProvider provider) =>
        TestDatabase.CreateAsync<ProbeDbContext>(provider, sqlServer, o => new ProbeDbContext(o), ProbeDbContext.HistoryTable);

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Only_the_active_providers_migrations_are_offered(TestProvider provider)
    {
        await using var database = await NewDatabase(provider);
        await using var db = database.CreateContext();

        var migrations = db.Database.GetMigrations().ToList();

        Assert.Single(migrations);
        Assert.EndsWith("_Baseline", migrations[0]);
        Assert.False(db.Database.HasPendingModelChanges());
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Fresh_database_migrates_into_its_own_history_table_and_keeps_decimals_exact(TestProvider provider)
    {
        await using var database = await NewDatabase(provider);
        await using (var db = database.CreateContext())
        {
            await db.Database.MigrateAsync();
            db.Rows.Add(new ProbeRow { Name = "a", Amount = 1234567.89m });
            db.Rows.Add(new ProbeRow { Name = "b", Amount = 2.50m });
            await db.SaveChangesAsync();
        }

        await using var read = database.CreateContext();
        Assert.Equal([1234567.89m, 2.50m], await read.Rows.OrderBy(x => x.Id).Select(x => x.Amount).ToListAsync());
        Assert.Equal(1, await database.ScalarAsync<int>($"SELECT COUNT(*) FROM {ProbeDbContext.HistoryTable}"));
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Adopting_an_existing_table_records_baseline_without_running_it_and_only_once(TestProvider provider)
    {
        await using var database = await NewDatabase(provider);
        var createTable = provider == TestProvider.Sqlite
            ? "CREATE TABLE ProbeRows (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Amount TEXT NOT NULL)"
            : "CREATE TABLE ProbeRows (Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProbeRows PRIMARY KEY, Name NVARCHAR(255) NOT NULL, Amount DECIMAL(20,9) NOT NULL)";
        await database.ExecuteSqlAsync(createTable);
        await database.ExecuteSqlAsync("INSERT INTO ProbeRows (Name, Amount) VALUES ('legacy', 5)");

        await using (var db = database.CreateContext())
        {
            Assert.True(await BaselineAdoption.AdoptAsync(db));
            Assert.False(await BaselineAdoption.AdoptAsync(db));
            await db.Database.MigrateAsync();
        }

        await using var read = database.CreateContext();
        Assert.Equal("legacy", (await read.Rows.SingleAsync()).Name);
        Assert.Single(await database.AppliedMigrationsAsync());
    }
}
