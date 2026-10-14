using UmbCheckout.Stripe.Migrations;
using UmbCheckout.Stripe.NotificationHandlers;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Testing;
using UmbCheckout.Tests.Infrastructure;
using Xunit;

namespace UmbCheckout.Tests.Persistence
{
    /// <summary>
    /// Group 5: the Stripe webhook records paid checkout sessions, so the customer's next basket read clears a basket they paid
    /// for without returning to the success page. Kept 30 days, like handled webhook events. Real NPoco on SQLite, table made by
    /// its migration.
    /// </summary>
    public class StripePaidCheckoutSessionsTests : IDisposable
    {
        private readonly NPocoTestDatabase _database;

        public StripePaidCheckoutSessionsTests() : this(new NPocoTestDatabase())
        {
        }

        protected StripePaidCheckoutSessionsTests(NPocoTestDatabase database)
        {
            _database = database;
            _database.Migrate(c => new AddUmbCheckoutStripePaidSessionsTable(c));
        }

        public void Dispose() => _database.Dispose();

        private StripePaidCheckoutSessions Store() => new(_database.ScopeProvider);

        private int Rows => _database.Scalar<int>("SELECT COUNT(*) FROM UmbCheckoutStripePaidSessions");

        [Fact]
        public async Task A_recorded_session_is_paid_and_any_other_is_not()
        {
            await Store().MarkPaidAsync("cs_test_paid");

            Assert.True(await Store().IsPaidAsync("cs_test_paid"));
            Assert.False(await Store().IsPaidAsync("cs_test_other"));
        }

        [Fact]
        public async Task Recording_a_session_twice_is_harmless()
        {
            // checkout.session.completed and async_payment_succeeded can both arrive for one session.
            await Store().MarkPaidAsync("cs_test_paid");
            await Store().MarkPaidAsync("cs_test_paid");

            Assert.True(await Store().IsPaidAsync("cs_test_paid"));
            Assert.Equal(1, Rows);
        }

        [Fact]
        public async Task A_concurrent_recording_of_the_same_session_is_harmless()
        {
            // Both deliveries found no row; the primary key lets one insert win, and the other is already done.
            _database.Execute("INSERT INTO UmbCheckoutStripePaidSessions (CheckoutSessionId, PaidUtc) VALUES ('cs_test_paid', @0)", DateTime.UtcNow);

            await Store().InsertOrConfirmAsync("cs_test_paid", DateTime.UtcNow);

            Assert.Equal(1, Rows);
            Assert.True(await Store().IsPaidAsync("cs_test_paid"));
        }

        [Fact]
        public async Task Sessions_older_than_30_days_are_deleted()
        {
            _database.Execute("INSERT INTO UmbCheckoutStripePaidSessions (CheckoutSessionId, PaidUtc) VALUES ('cs_test_old', @0)", DateTime.UtcNow.AddDays(-31));
            _database.Execute("INSERT INTO UmbCheckoutStripePaidSessions (CheckoutSessionId, PaidUtc) VALUES ('cs_test_recent', @0)", DateTime.UtcNow.AddDays(-29));

            await Store().MarkPaidAsync("cs_test_new");

            Assert.False(await Store().IsPaidAsync("cs_test_old"));
            Assert.True(await Store().IsPaidAsync("cs_test_recent"));
            Assert.True(await Store().IsPaidAsync("cs_test_new"));
        }

        [Fact]
        public void The_table_comes_from_its_own_open_plan()
        {
            var plan = RunUmbCheckoutStripePaidSessionsMigration.CreatePlan();

            Assert.Equal(typeof(AddUmbCheckoutStripePaidSessionsTable), plan.Transitions[string.Empty]!.MigrationType);
            Assert.Equal(plan.Transitions[string.Empty]!.TargetState, plan.FinalState);
        }

        [Fact]
        public void The_migration_is_idempotent()
        {
            _database.Migrate(c => new AddUmbCheckoutStripePaidSessionsTable(c));

            Assert.Equal(0, Rows);
        }
    }

    /// <summary>The same cases against SQL Server in a container (needs Docker).</summary>
    [Trait("Category", "Integration")]
    [Collection(DatabaseCollection.Name)]
    public sealed class StripePaidCheckoutSessionsSqlServerTests : StripePaidCheckoutSessionsTests
    {
        public StripePaidCheckoutSessionsSqlServerTests(SqlServerContainerFixture sqlServer) : base(NPocoTestDatabase.SqlServer(sqlServer))
        {
        }
    }
}
