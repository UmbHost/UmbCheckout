using Microsoft.EntityFrameworkCore;
using UmbCheckout.Stripe.Persistence;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Testing;
using Xunit;

namespace UmbCheckout.Tests.Persistence;

/// <summary>
/// Sub-project B: the Stripe webhook records paid checkout sessions, so the customer's next basket read clears a basket they paid
/// for without returning to the success page. Kept 30 days, like handled webhook events.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class StripePaidCheckoutSessionsTests(SqlServerContainerFixture sqlServer)
{
    public static TheoryData<TestProvider> Providers => new() { TestProvider.Sqlite, TestProvider.SqlServer };

    private sealed class Clock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;

        public override DateTimeOffset GetUtcNow() => Now;
    }

    private readonly Clock _clock = new(new DateTimeOffset(2026, 10, 13, 9, 0, 0, TimeSpan.Zero));

    private async Task<(TestDatabase<UmbCheckoutStripeDbContext> Database, StripePaidCheckoutSessions Store)> Arrange(TestProvider provider)
    {
        var database = await TestDatabase.CreateAsync<UmbCheckoutStripeDbContext>(provider, sqlServer, o => new UmbCheckoutStripeDbContext(o), UmbCheckoutStripeDbContext.HistoryTable);
        await using (var db = database.CreateContext())
        {
            await db.Database.MigrateAsync();
        }

        return (database, new StripePaidCheckoutSessions(database.ScopeProvider, _clock));
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_recorded_session_is_paid_and_any_other_is_not(TestProvider provider)
    {
        var (database, store) = await Arrange(provider);
        await using var _ = database;

        await store.MarkPaidAsync("cs_test_paid");

        Assert.True(await store.IsPaidAsync("cs_test_paid"));
        Assert.False(await store.IsPaidAsync("cs_test_other"));
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Recording_a_session_twice_is_harmless(TestProvider provider)
    {
        // checkout.session.completed and async_payment_succeeded can both arrive for one session.
        var (database, store) = await Arrange(provider);
        await using var _ = database;

        await store.MarkPaidAsync("cs_test_paid");
        await store.MarkPaidAsync("cs_test_paid");

        Assert.True(await store.IsPaidAsync("cs_test_paid"));
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Sessions_older_than_30_days_are_deleted(TestProvider provider)
    {
        var (database, store) = await Arrange(provider);
        await using var _ = database;
        await store.MarkPaidAsync("cs_test_old");
        _clock.Now += TimeSpan.FromDays(31);

        await store.MarkPaidAsync("cs_test_new");

        Assert.False(await store.IsPaidAsync("cs_test_old"));
        Assert.True(await store.IsPaidAsync("cs_test_new"));
    }
}
