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
[Trait("Category", "Integration")] // SQL Server cases need Docker
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

        var claim = await store.ClaimAsync("evt_1");
        Assert.Equal(StripeWebhookEventClaim.Claimed, claim.Claim);
        Assert.False(string.IsNullOrEmpty(claim.LeaseToken));
        await store.CompleteAsync("evt_1", claim.LeaseToken!);

        Assert.Equal(StripeWebhookEventClaim.AlreadyHandled, (await store.ClaimAsync("evt_1")).Claim);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_redelivery_while_the_first_is_running_is_told_to_retry_later(TestProvider provider)
    {
        var (database, store) = await Arrange(provider);
        await using var _ = database;

        Assert.Equal(StripeWebhookEventClaim.Claimed, (await store.ClaimAsync("evt_1")).Claim);

        Assert.Equal(StripeWebhookEventClaim.InProgress, (await store.ClaimAsync("evt_1")).Claim);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task An_attempt_that_died_is_taken_over_once_its_lease_expires(TestProvider provider)
    {
        // A crash, restart or timeout leaves the claim behind; it must not swallow every retry.
        var (database, store) = await Arrange(provider, lease: TimeSpan.Zero);
        await using var _ = database;

        Assert.Equal(StripeWebhookEventClaim.Claimed, (await store.ClaimAsync("evt_1")).Claim);

        Assert.Equal(StripeWebhookEventClaim.Claimed, (await store.ClaimAsync("evt_1")).Claim);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_forgotten_event_is_claimed_again(TestProvider provider)
    {
        var (database, store) = await Arrange(provider);
        await using var _ = database;

        var claim = await store.ClaimAsync("evt_1");
        await store.ForgetAsync("evt_1", claim.LeaseToken!);

        Assert.Equal(StripeWebhookEventClaim.Claimed, (await store.ClaimAsync("evt_1")).Claim);
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

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_stale_attempt_cannot_release_the_claim_of_the_delivery_that_took_over(TestProvider provider)
    {
        // The first attempt outlives its lease, a retry takes the event over, then the first attempt's handler fails and it
        // releases "its" claim. It must not delete the retry's claim (a third delivery would run the handlers alongside the retry).
        var (database, store) = await Arrange(provider, lease: TimeSpan.Zero);
        await using var _ = database;
        var stale = await store.ClaimAsync("evt_1");
        var takeover = await store.ClaimAsync("evt_1");
        Assert.Equal(StripeWebhookEventClaim.Claimed, takeover.Claim);
        Assert.NotEqual(stale.LeaseToken, takeover.LeaseToken);

        await store.ForgetAsync("evt_1", stale.LeaseToken!);

        Assert.Equal(1, await database.ScalarAsync<int>("SELECT COUNT(*) FROM UmbCheckoutStripeWebhookEvents"));
        Assert.Equal(takeover.LeaseToken, await database.ScalarAsync<string>("SELECT LeaseToken FROM UmbCheckoutStripeWebhookEvents WHERE EventId = 'evt_1'"));
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_stale_attempt_cannot_complete_the_claim_of_the_delivery_that_took_over(TestProvider provider)
    {
        var (database, store) = await Arrange(provider, lease: TimeSpan.Zero);
        await using var _ = database;
        var stale = await store.ClaimAsync("evt_1");
        var takeover = await store.ClaimAsync("evt_1");

        await store.CompleteAsync("evt_1", stale.LeaseToken!);

        Assert.Equal(0, await database.ScalarAsync<int>("SELECT Status FROM UmbCheckoutStripeWebhookEvents WHERE EventId = 'evt_1'"));
        await store.CompleteAsync("evt_1", takeover.LeaseToken!);
        Assert.Equal(1, await database.ScalarAsync<int>("SELECT Status FROM UmbCheckoutStripeWebhookEvents WHERE EventId = 'evt_1'"));
    }
}
