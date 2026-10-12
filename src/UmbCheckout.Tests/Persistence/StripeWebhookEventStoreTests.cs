using Microsoft.EntityFrameworkCore;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Persistence;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Testing;
using Xunit;

namespace UmbCheckout.Tests.Persistence;

/// <summary>
/// Stripe delivers each webhook event at least once and retries for up to 3 days. An event is claimed with a lease while its
/// handlers run and marked done after they succeed: a redelivery of a done event is acknowledged, one that arrives while the
/// first is still running is told to retry later, and one whose first attempt died (lease expired) takes the event over.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class StripeWebhookEventStoreTests(SqlServerContainerFixture sqlServer)
{
    public static TheoryData<TestProvider> Providers => new() { TestProvider.Sqlite, TestProvider.SqlServer };

    private async Task<(TestDatabase<UmbCheckoutStripeDbContext> Database, StripeWebhookEventStore Store)> Arrange(TestProvider provider, TimeSpan? lease = null)
    {
        var database = await TestDatabase.CreateAsync<UmbCheckoutStripeDbContext>(provider, sqlServer, o => new UmbCheckoutStripeDbContext(o), UmbCheckoutStripeDbContext.HistoryTable);
        await using (var db = database.CreateContext())
        {
            await db.Database.MigrateAsync();
        }

        return (database, new StripeWebhookEventStore(database.ScopeProvider, lease ?? TimeSpan.FromMinutes(10)));
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_completed_event_is_acknowledged_on_redelivery(TestProvider provider)
    {
        var (database, store) = await Arrange(provider);
        await using var _ = database;

        Assert.Equal(StripeWebhookEventClaim.Claimed, await store.TryClaimAsync("evt_1"));
        await store.CompleteAsync("evt_1");

        Assert.Equal(StripeWebhookEventClaim.AlreadyHandled, await store.TryClaimAsync("evt_1"));
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_redelivery_while_the_first_is_running_is_told_to_retry_later(TestProvider provider)
    {
        var (database, store) = await Arrange(provider);
        await using var _ = database;

        Assert.Equal(StripeWebhookEventClaim.Claimed, await store.TryClaimAsync("evt_1"));

        Assert.Equal(StripeWebhookEventClaim.InProgress, await store.TryClaimAsync("evt_1"));
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task An_attempt_that_died_is_taken_over_once_its_lease_expires(TestProvider provider)
    {
        // A crash, restart or timeout leaves the claim behind; it must not swallow every retry.
        var (database, store) = await Arrange(provider, lease: TimeSpan.Zero);
        await using var _ = database;

        Assert.Equal(StripeWebhookEventClaim.Claimed, await store.TryClaimAsync("evt_1"));

        Assert.Equal(StripeWebhookEventClaim.Claimed, await store.TryClaimAsync("evt_1"));
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_forgotten_event_is_claimed_again(TestProvider provider)
    {
        var (database, store) = await Arrange(provider);
        await using var _ = database;

        await store.TryClaimAsync("evt_1");
        await store.ForgetAsync("evt_1");

        Assert.Equal(StripeWebhookEventClaim.Claimed, await store.TryClaimAsync("evt_1"));
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Records_older_than_30_days_are_pruned(TestProvider provider)
    {
        var (database, store) = await Arrange(provider);
        await using var _ = database;
        await database.ExecuteSqlAsync("INSERT INTO UmbCheckoutStripeWebhookEvents (EventId, ReceivedUtc, Status) VALUES ('evt_old', '2020-01-01 00:00:00', 1)");

        await store.TryClaimAsync("evt_new");

        Assert.Equal(1, await database.ScalarAsync<int>("SELECT COUNT(*) FROM UmbCheckoutStripeWebhookEvents"));
    }

    [Fact]
    public async Task A_save_failure_that_is_not_a_redelivery_is_not_reported_as_one()
    {
        // SQL Server refuses an id longer than the column: a DbUpdateException that isn't a duplicate. Answering
        // "already handled" would drop the event; it must surface so the webhook answers 500 and Stripe retries.
        var (database, store) = await Arrange(TestProvider.SqlServer);
        await using var _ = database;

        await Assert.ThrowsAsync<DbUpdateException>(() => store.TryClaimAsync(new string('x', 300)));
    }
}
