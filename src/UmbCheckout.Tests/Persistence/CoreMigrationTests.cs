using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UmbCheckout.Core.Persistence;
using UmbCheckout.Shared.Persistence;
using UmbCheckout.Testing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Xunit;

namespace UmbCheckout.Tests.Persistence;

[Collection(DatabaseCollection.Name)]
public sealed class CoreMigrationTests(SqlServerContainerFixture sqlServer)
{
    public static TheoryData<TestProvider> Providers => new() { TestProvider.Sqlite, TestProvider.SqliteSharedCacheFile, TestProvider.SqlServer };

    private sealed class RecordingRunner : ILegacyMigrationPlanRunner
    {
        public List<string> Ran { get; } = [];

        public Task RunAsync(MigrationPlan plan, CancellationToken cancellationToken = default)
        {
            Ran.Add(plan.Name);
            return Task.CompletedTask;
        }
    }

    private static UmbCheckoutDatabaseMigrator Migrator(TestDatabase<UmbCheckoutDbContext> database, bool legacy, RecordingRunner runner)
    {
        var runtime = Mock.Of<IRuntimeState>(r => r.Level == RuntimeLevel.Run);
        var keyValues = new Mock<IKeyValueService>();
        keyValues.Setup(k => k.GetValue(Constants.Conventions.Migrations.KeyValuePrefix + "UmbCheckoutConfiguration"))
            .Returns(legacy ? "4e8a2c1d-6b5f-4a3e-8d7c-9f0e1a2b3c4d" : null);
        return new UmbCheckoutDatabaseMigrator(database.ContextFactory, runtime, keyValues.Object, runner, NullLogger<UmbCheckoutDatabaseMigrator>.Instance);
    }

    private static string Fixture(TestProvider provider) =>
        Path.Combine(AppContext.BaseDirectory, "Persistence", "Fixtures", provider != TestProvider.SqlServer ? "legacy-sqlite.sql" : "legacy-sqlserver.sql");

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Fresh_install_creates_the_table_from_baseline_and_matches_the_model(TestProvider provider)
    {
        await using var database = await TestDatabase.CreateAsync<UmbCheckoutDbContext>(provider, sqlServer, o => new UmbCheckoutDbContext(o), UmbCheckoutDbContext.HistoryTable);
        var runner = new RecordingRunner();

        await Migrator(database, legacy: false, runner).MigrateAsync();

        Assert.Empty(runner.Ran);
        Assert.Contains(await database.AppliedMigrationsAsync(), id => id.EndsWith("_Baseline"));
        await using var db = database.CreateContext();
        Assert.False(db.Database.HasPendingModelChanges());
        Assert.Empty(await db.Configurations.ToListAsync());
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Legacy_install_is_adopted_its_rows_survive_and_a_second_run_changes_nothing(TestProvider provider)
    {
        await using var database = await TestDatabase.CreateAsync<UmbCheckoutDbContext>(provider, sqlServer, o => new UmbCheckoutDbContext(o), UmbCheckoutDbContext.HistoryTable);
        await database.ExecuteSqlFileAsync(Fixture(provider));
        await database.ExecuteSqlAsync("INSERT INTO UmbCheckoutConfiguration (SuccessPageUrl, CancelPageUrl, StoreBasketInCookie, StoreBasketInDatabase, BasketInDatabaseExpiry, BasketInCookieExpiry, EnableShipping, \"Key\", CurrencyCode) VALUES ('/thanks', '/basket', 1, 0, 30, 14, 1, NULL, 'GBP')");
        var runner = new RecordingRunner();
        var migrator = Migrator(database, legacy: true, runner);

        await migrator.MigrateAsync();
        await migrator.MigrateAsync();

        Assert.Equal(["UmbCheckoutConfiguration", "UmbCheckoutConfiguration"], runner.Ran);
        Assert.Single(await database.AppliedMigrationsAsync(), id => id.EndsWith("_Baseline"));
        await using var db = database.CreateContext();
        var row = await db.Configurations.SingleAsync();
        Assert.Null(row.Key);
        Assert.Equal("/thanks", row.SuccessPageUrl);
        Assert.Equal(14, row.BasketInCookieExpiry);
    }
}
