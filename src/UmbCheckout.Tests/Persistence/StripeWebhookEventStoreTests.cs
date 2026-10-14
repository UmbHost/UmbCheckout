using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Migrations;
using UmbCheckout.Stripe.NotificationHandlers;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Testing;
using UmbCheckout.Tests.Infrastructure;
using Xunit;

namespace UmbCheckout.Tests.Persistence
{
    /// <summary>
    /// Stripe delivers each webhook event at least once: an event is claimed with a lease while its handlers run and marked done
    /// after, so a redelivery is acknowledged without running handlers again. Real NPoco on SQLite, table made by its migration.
    /// </summary>
    public class StripeWebhookEventStoreTests : IDisposable
    {
        private readonly NPocoTestDatabase _database;

        public StripeWebhookEventStoreTests() : this(new NPocoTestDatabase())
        {
        }

        /// <summary>A second, empty database on the engine under test.</summary>
        protected virtual NPocoTestDatabase NewDatabase() => new();

        protected StripeWebhookEventStoreTests(NPocoTestDatabase database)
        {
            _database = database;
            _database.Migrate(c => new AddUmbCheckoutStripeWebhookEventsTable(c), c => new AddUmbCheckoutStripeWebhookEventLeaseToken(c));
        }

        public void Dispose() => _database.Dispose();

        private StripeWebhookEventStore Store(TimeSpan? lease = null) => new(_database.ScopeProvider, lease ?? TimeSpan.FromMinutes(10));

        private int Rows => _database.Scalar<int>("SELECT COUNT(*) FROM UmbCheckoutStripeWebhookEvents");

        [Fact]
        public async Task An_event_is_claimed_once_and_acknowledged_once_done()
        {
            var store = Store();

            var claim = await store.TryClaimAsync("evt_1");
            Assert.Equal(StripeWebhookEventClaim.Claimed, claim.Claim);
            Assert.False(string.IsNullOrEmpty(claim.LeaseToken));
            Assert.Equal(StripeWebhookEventClaim.InProgress, (await store.TryClaimAsync("evt_1")).Claim);
            await store.CompleteAsync("evt_1", claim.LeaseToken.Required());
            Assert.Equal(StripeWebhookEventClaim.AlreadyHandled, (await store.TryClaimAsync("evt_1")).Claim);
        }

        [Fact]
        public async Task A_forgotten_event_is_claimed_again()
        {
            var store = Store();
            var claim = await store.TryClaimAsync("evt_1");

            await store.ForgetAsync("evt_1", claim.LeaseToken.Required());

            Assert.Equal(StripeWebhookEventClaim.Claimed, (await store.TryClaimAsync("evt_1")).Claim);
        }

        [Fact]
        public async Task An_attempt_whose_lease_expired_is_taken_over()
        {
            // The first delivery died mid-way (crash, timeout): its retry must run the handlers, not be lost.
            var store = Store(TimeSpan.Zero);
            await store.TryClaimAsync("evt_1");

            Assert.Equal(StripeWebhookEventClaim.Claimed, (await store.TryClaimAsync("evt_1")).Claim);
        }

        [Fact]
        public async Task A_done_event_is_never_taken_over()
        {
            var store = Store(TimeSpan.Zero);
            var claim = await store.TryClaimAsync("evt_1");
            await store.CompleteAsync("evt_1", claim.LeaseToken.Required());

            Assert.Equal(StripeWebhookEventClaim.AlreadyHandled, (await store.TryClaimAsync("evt_1")).Claim);
        }

        [Fact]
        public async Task A_stale_attempt_cannot_release_the_claim_of_the_delivery_that_took_over()
        {
            // Group 4 review M4: the first attempt outlives its lease, a retry takes the event over, then the first attempt's
            // handler fails and it releases "its" claim. It must not delete the retry's claim (a third delivery would run the
            // handlers alongside the retry).
            var store = Store(TimeSpan.Zero);
            var stale = await store.TryClaimAsync("evt_1");
            var takeover = await store.TryClaimAsync("evt_1");
            Assert.Equal(StripeWebhookEventClaim.Claimed, takeover.Claim);
            Assert.NotEqual(stale.LeaseToken, takeover.LeaseToken);

            await store.ForgetAsync("evt_1", stale.LeaseToken.Required());

            Assert.Equal(1, Rows);
            Assert.Equal(takeover.LeaseToken, _database.Scalar<string>("SELECT LeaseToken FROM UmbCheckoutStripeWebhookEvents WHERE EventId = 'evt_1'"));
        }

        [Fact]
        public async Task A_stale_attempt_cannot_complete_the_claim_of_the_delivery_that_took_over()
        {
            var store = Store(TimeSpan.Zero);
            var stale = await store.TryClaimAsync("evt_1");
            var takeover = await store.TryClaimAsync("evt_1");

            await store.CompleteAsync("evt_1", stale.LeaseToken.Required());

            Assert.Equal(0, _database.Scalar<int>("SELECT Status FROM UmbCheckoutStripeWebhookEvents WHERE EventId = 'evt_1'"));
            await store.CompleteAsync("evt_1", takeover.LeaseToken.Required());
            Assert.Equal(StripeWebhookEventClaim.AlreadyHandled, (await store.TryClaimAsync("evt_1")).Claim);
        }

        [Fact]
        public async Task The_delivery_that_took_over_releases_its_own_claim()
        {
            var store = Store(TimeSpan.Zero);
            await store.TryClaimAsync("evt_1");
            var takeover = await store.TryClaimAsync("evt_1");

            await store.ForgetAsync("evt_1", takeover.LeaseToken.Required());

            Assert.Equal(0, Rows);
        }

        [Fact]
        public async Task Records_older_than_30_days_are_pruned()
        {
            _database.Execute("INSERT INTO UmbCheckoutStripeWebhookEvents (EventId, ReceivedUtc, Status) VALUES ('evt_old', @0, 1)", DateTime.UtcNow.AddDays(-31));
            _database.Execute("INSERT INTO UmbCheckoutStripeWebhookEvents (EventId, ReceivedUtc, Status) VALUES ('evt_recent', @0, 1)", DateTime.UtcNow.AddDays(-29));

            await Store().TryClaimAsync("evt_new");

            Assert.Equal(2, Rows);
            Assert.Equal(StripeWebhookEventClaim.AlreadyHandled, (await Store().TryClaimAsync("evt_recent")).Claim);
        }

        [Fact]
        public async Task A_redelivery_racing_the_first_insert_is_in_progress()
        {
            // Two servers (or two deliveries) both find no row; the primary key lets one insert, the other answers "in progress".
            _database.Execute("INSERT INTO UmbCheckoutStripeWebhookEvents (EventId, ReceivedUtc, Status, LeaseUntilUtc) VALUES ('evt_1', @0, 0, @1)", DateTime.UtcNow, DateTime.UtcNow.AddMinutes(10));

            Assert.Equal(StripeWebhookEventClaim.InProgress, (await Store().InsertOrReportAsync("evt_1", DateTime.UtcNow, "token", CancellationToken.None)).Claim);
        }

        [Fact]
        public void The_table_comes_from_the_open_plan_on_every_site()
        {
            var plan = RunUmbCheckoutStripeWebhookEventsMigration.CreatePlan();

            Assert.Equal(typeof(AddUmbCheckoutStripeWebhookEventsTable), plan.Transitions[string.Empty].Required().MigrationType);
        }

        [Fact]
        public void The_migration_is_idempotent()
        {
            _database.Migrate(c => new AddUmbCheckoutStripeWebhookEventsTable(c), c => new AddUmbCheckoutStripeWebhookEventLeaseToken(c));

            Assert.Equal(0, Rows);
        }

        [Fact]
        public void The_lease_token_column_is_added_to_a_table_created_without_it()
        {
            // A site that ran the first step before the lease token existed: the second step adds the column.
            using var database = NewDatabase();
            database.Execute(database.Provider == TestProvider.Sqlite
                ? "CREATE TABLE UmbCheckoutStripeWebhookEvents (EventId TEXT NOT NULL PRIMARY KEY, ReceivedUtc TEXT NOT NULL, Status INTEGER NOT NULL, LeaseUntilUtc TEXT NULL)"
                : "CREATE TABLE UmbCheckoutStripeWebhookEvents (EventId NVARCHAR(255) NOT NULL PRIMARY KEY, ReceivedUtc DATETIME2 NOT NULL, Status INT NOT NULL, LeaseUntilUtc DATETIME2 NULL)");

            database.Migrate(c => new AddUmbCheckoutStripeWebhookEventLeaseToken(c));

            Assert.Equal(0, database.Scalar<int>("SELECT COUNT(LeaseToken) FROM UmbCheckoutStripeWebhookEvents"));
        }

        [Fact]
        public void The_lease_token_comes_from_the_open_plan_after_the_table()
        {
            var plan = RunUmbCheckoutStripeWebhookEventsMigration.CreatePlan();
            var first = plan.Transitions[string.Empty].Required().TargetState;

            Assert.Equal(typeof(AddUmbCheckoutStripeWebhookEventLeaseToken), plan.Transitions[first].Required().MigrationType);
            Assert.Equal(plan.Transitions[first].Required().TargetState, plan.FinalState);
        }
    }

    /// <summary>The same cases against SQL Server in a container (needs Docker).</summary>
    [Trait("Category", "Integration")]
    [Collection(DatabaseCollection.Name)]
    public sealed class StripeWebhookEventStoreSqlServerTests : StripeWebhookEventStoreTests
    {
        private readonly SqlServerContainerFixture _sqlServer;

        public StripeWebhookEventStoreSqlServerTests(SqlServerContainerFixture sqlServer) : base(NPocoTestDatabase.SqlServer(sqlServer)) => _sqlServer = sqlServer;

        protected override NPocoTestDatabase NewDatabase() => NPocoTestDatabase.SqlServer(_sqlServer);
    }
}
