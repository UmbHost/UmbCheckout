using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UmbCheckout.Shared.Persistence;
using UmbCheckout.Stripe.Persistence;
using UmbCheckout.Testing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Xunit;

namespace UmbCheckout.Tests.Persistence;

[Collection(DatabaseCollection.Name)]
public sealed class StripeMigrationTests(SqlServerContainerFixture sqlServer)
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

    private static UmbCheckoutStripeDatabaseMigrator Migrator(TestDatabase<UmbCheckoutStripeDbContext> database, bool legacy, RecordingRunner runner)
    {
        var runtime = Mock.Of<IRuntimeState>(r => r.Level == RuntimeLevel.Run);
        var keyValues = new Mock<IKeyValueService>();
        keyValues.Setup(k => k.GetValue(Constants.Conventions.Migrations.KeyValuePrefix + "UmbCheckoutStripeSettings"))
            .Returns(legacy ? "05b38788-64d1-41cc-9f8c-3e3900127140" : null);
        keyValues.Setup(k => k.GetValue(Constants.Conventions.Migrations.KeyValuePrefix + "UmbCheckoutStripeShipping"))
            .Returns(legacy ? "9219beca-8c25-4397-8394-ae9dd51ffbee" : null);
        return new UmbCheckoutStripeDatabaseMigrator(database.ContextFactory, runtime, keyValues.Object, runner, NullLogger<UmbCheckoutStripeDatabaseMigrator>.Instance);
    }

    private static string Fixture(TestProvider provider) =>
        Path.Combine(AppContext.BaseDirectory, "Persistence", "Fixtures", provider != TestProvider.SqlServer ? "legacy-sqlite.sql" : "legacy-sqlserver.sql");

    private static Task<TestDatabase<UmbCheckoutStripeDbContext>> NewDatabase(TestProvider provider, SqlServerContainerFixture sqlServer) =>
        TestDatabase.CreateAsync<UmbCheckoutStripeDbContext>(provider, sqlServer, o => new UmbCheckoutStripeDbContext(o), UmbCheckoutStripeDbContext.HistoryTable);

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Fresh_install_creates_both_tables_from_baseline_and_matches_the_model(TestProvider provider)
    {
        await using var database = await NewDatabase(provider, sqlServer);
        var runner = new RecordingRunner();

        await Migrator(database, legacy: false, runner).MigrateAsync();

        Assert.Empty(runner.Ran);
        Assert.Contains(await database.AppliedMigrationsAsync(), id => id.EndsWith("_Baseline"));
        await using var db = database.CreateContext();
        Assert.False(db.Database.HasPendingModelChanges());
        Assert.Empty(await db.StripeSettings.ToListAsync());
        Assert.Empty(await db.ShippingRates.ToListAsync());
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Legacy_install_runs_both_plans_adopts_once_and_keeps_rows_and_unique_indexes(TestProvider provider)
    {
        await using var database = await NewDatabase(provider, sqlServer);
        await database.ExecuteSqlFileAsync(Fixture(provider));
        await database.ExecuteSqlAsync("INSERT INTO UmbCheckoutStripeSettings (\"Key\", UseLiveApiDetails, ShippingAllowedCountries, CollectPhoneNumber, CollectPromotionalConsent, AllowPromotionalCodes, EnableAbandonedCartRecovery, AllowPromotionalCodesOnRecoveredCarts) VALUES (NULL, 0, 'GB', 1, 0, 1, 0, 0)");
        await database.ExecuteSqlAsync("INSERT INTO UmbCheckoutStripeShipping (Name, Value, \"Key\") VALUES ('Standard', 'shr_1', NULL)");
        await database.ExecuteSqlAsync("INSERT INTO UmbCheckoutStripeShipping (Name, Value, \"Key\") VALUES ('Express', 'shr_2', '6F1D2C3B-0000-4000-8000-000000000001')");
        var runner = new RecordingRunner();
        var migrator = Migrator(database, legacy: true, runner);

        await migrator.MigrateAsync();
        await migrator.MigrateAsync();

        Assert.Equal(["UmbCheckoutStripeSettings", "UmbCheckoutStripeShipping", "UmbCheckoutStripeSettings", "UmbCheckoutStripeShipping"], runner.Ran);
        Assert.Single(await database.AppliedMigrationsAsync(), id => id.EndsWith("_Baseline"));
        await using var db = database.CreateContext();
        var settings = await db.StripeSettings.SingleAsync();
        Assert.Equal("GB", settings.ShippingAllowedCountries);
        Assert.True(settings.CollectPhoneNumber);
        var rates = await db.ShippingRates.OrderBy(x => x.Id).ToListAsync();
        Assert.Null(rates[0].Key);
        Assert.Equal(Guid.Parse("6F1D2C3B-0000-4000-8000-000000000001"), rates[1].Key);

        db.ShippingRates.Add(new UmbCheckout.Stripe.Pocos.UmbCheckoutStripeShipping { Name = "Dup", Value = "shr_1" });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
}
