using Microsoft.EntityFrameworkCore;
using UmbCheckout.Stripe.Persistence;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Testing;
using Xunit;

namespace UmbCheckout.Tests.Persistence;

/// <summary>
/// Stripe delivers each webhook event at least once (it retries for up to 3 days), so handled event ids are recorded and a
/// redelivery is recognised; a failed handler forgets its event so Stripe's retry runs it again.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class StripeWebhookEventStoreTests(SqlServerContainerFixture sqlServer)
{
    public static TheoryData<TestProvider> Providers => new() { TestProvider.Sqlite, TestProvider.SqlServer };

    private async Task<(TestDatabase<UmbCheckoutStripeDbContext> Database, StripeWebhookEventStore Store)> Arrange(TestProvider provider)
    {
        var database = await TestDatabase.CreateAsync<UmbCheckoutStripeDbContext>(provider, sqlServer, o => new UmbCheckoutStripeDbContext(o), UmbCheckoutStripeDbContext.HistoryTable);
        await using (var db = database.CreateContext())
        {
            await db.Database.MigrateAsync();
        }

        return (database, new StripeWebhookEventStore(database.ScopeProvider));
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task An_event_is_begun_once_until_it_is_forgotten(TestProvider provider)
    {
        var (database, store) = await Arrange(provider);
        await using var _ = database;

        Assert.True(await store.TryBeginAsync("evt_1"));
        Assert.False(await store.TryBeginAsync("evt_1"));
        await store.ForgetAsync("evt_1");
        Assert.True(await store.TryBeginAsync("evt_1"));
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Records_older_than_30_days_are_pruned(TestProvider provider)
    {
        var (database, store) = await Arrange(provider);
        await using var _ = database;
        await database.ExecuteSqlAsync("INSERT INTO UmbCheckoutStripeWebhookEvents (EventId, ReceivedUtc) VALUES ('evt_old', '2020-01-01 00:00:00')");

        await store.TryBeginAsync("evt_new");

        Assert.Equal(1, await database.ScalarAsync<int>("SELECT COUNT(*) FROM UmbCheckoutStripeWebhookEvents"));
    }
}
