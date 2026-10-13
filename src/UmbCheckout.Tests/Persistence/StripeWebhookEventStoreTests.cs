using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Migrations;
using UmbCheckout.Stripe.NotificationHandlers;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Tests.Infrastructure;
using Xunit;

namespace UmbCheckout.Tests.Persistence
{
    /// <summary>
    /// Stripe delivers each webhook event at least once: an event is claimed with a lease while its handlers run and marked done
    /// after, so a redelivery is acknowledged without running handlers again. Real NPoco on SQLite, table made by its migration.
    /// </summary>
    public sealed class StripeWebhookEventStoreTests : IDisposable
    {
        private readonly SqliteTestDatabase _database = new();

        public StripeWebhookEventStoreTests() => _database.Migrate(c => new AddUmbCheckoutStripeWebhookEventsTable(c));

        public void Dispose() => _database.Dispose();

        private StripeWebhookEventStore Store(TimeSpan? lease = null) => new(_database.ScopeProvider, lease ?? TimeSpan.FromMinutes(10));

        private int Rows => _database.Scalar<int>("SELECT COUNT(*) FROM UmbCheckoutStripeWebhookEvents");

        [Fact]
        public async Task An_event_is_claimed_once_and_acknowledged_once_done()
        {
            var store = Store();

            Assert.Equal(StripeWebhookEventClaim.Claimed, await store.TryClaimAsync("evt_1"));
            Assert.Equal(StripeWebhookEventClaim.InProgress, await store.TryClaimAsync("evt_1"));
            await store.CompleteAsync("evt_1");
            Assert.Equal(StripeWebhookEventClaim.AlreadyHandled, await store.TryClaimAsync("evt_1"));
        }

        [Fact]
        public async Task A_forgotten_event_is_claimed_again()
        {
            var store = Store();
            await store.TryClaimAsync("evt_1");

            await store.ForgetAsync("evt_1");

            Assert.Equal(StripeWebhookEventClaim.Claimed, await store.TryClaimAsync("evt_1"));
        }

        [Fact]
        public async Task An_attempt_whose_lease_expired_is_taken_over()
        {
            // The first delivery died mid-way (crash, timeout): its retry must run the handlers, not be lost.
            var store = Store(TimeSpan.Zero);
            await store.TryClaimAsync("evt_1");

            Assert.Equal(StripeWebhookEventClaim.Claimed, await store.TryClaimAsync("evt_1"));
        }

        [Fact]
        public async Task A_done_event_is_never_taken_over()
        {
            var store = Store(TimeSpan.Zero);
            await store.TryClaimAsync("evt_1");
            await store.CompleteAsync("evt_1");

            Assert.Equal(StripeWebhookEventClaim.AlreadyHandled, await store.TryClaimAsync("evt_1"));
        }

        [Fact]
        public async Task Records_older_than_30_days_are_pruned()
        {
            _database.Execute("INSERT INTO UmbCheckoutStripeWebhookEvents (EventId, ReceivedUtc, Status) VALUES ('evt_old', @0, 1)", DateTime.UtcNow.AddDays(-31));
            _database.Execute("INSERT INTO UmbCheckoutStripeWebhookEvents (EventId, ReceivedUtc, Status) VALUES ('evt_recent', @0, 1)", DateTime.UtcNow.AddDays(-29));

            await Store().TryClaimAsync("evt_new");

            Assert.Equal(2, Rows);
            Assert.Equal(StripeWebhookEventClaim.AlreadyHandled, await Store().TryClaimAsync("evt_recent"));
        }

        [Fact]
        public async Task A_redelivery_racing_the_first_insert_is_in_progress()
        {
            // Two servers (or two deliveries) both find no row; the primary key lets one insert, the other answers "in progress".
            _database.Execute("INSERT INTO UmbCheckoutStripeWebhookEvents (EventId, ReceivedUtc, Status, LeaseUntilUtc) VALUES ('evt_1', @0, 0, @1)", DateTime.UtcNow, DateTime.UtcNow.AddMinutes(10));

            Assert.Equal(StripeWebhookEventClaim.InProgress, await Store().InsertOrReportAsync("evt_1", DateTime.UtcNow, CancellationToken.None));
        }

        [Fact]
        public void The_table_comes_from_the_open_plan_on_every_site()
        {
            var plan = RunUmbCheckoutStripeWebhookEventsMigration.CreatePlan();

            Assert.Equal(typeof(AddUmbCheckoutStripeWebhookEventsTable), plan.Transitions[string.Empty]!.MigrationType);
            Assert.Equal(plan.Transitions[string.Empty]!.TargetState, plan.FinalState);
        }

        [Fact]
        public void The_migration_is_idempotent()
        {
            _database.Migrate(c => new AddUmbCheckoutStripeWebhookEventsTable(c));

            Assert.Equal(0, Rows);
        }
    }
}
