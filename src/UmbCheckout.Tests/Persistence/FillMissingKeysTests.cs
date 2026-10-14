using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UmbCheckout.Stripe.Composers;
using UmbCheckout.Stripe.Migrations;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Testing;
using UmbCheckout.Tests.Infrastructure;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Scoping;
using Xunit;

namespace UmbCheckout.Tests.Persistence
{
    /// <summary>
    /// Shipping rates created before the Key column existed have a NULL Key, which the backoffice can't look up (it finds rates by Key).
    /// A migration step gives each its own key. Real NPoco on SQLite.
    /// </summary>
    public class FillMissingKeysTests : IDisposable
    {
        private readonly NPocoTestDatabase _database;

        public FillMissingKeysTests() : this(new NPocoTestDatabase())
        {
        }

        protected FillMissingKeysTests(NPocoTestDatabase database) => _database = database;

        public void Dispose() => _database.Dispose();

        [Fact]
        public async Task Legacy_shipping_rates_without_a_key_get_their_own_key_so_the_backoffice_can_reach_them()
        {
            _database.Migrate(c => new AddUmbCheckoutStripeShippingTable(c), c => new AddStripeShippingKey(c));
            _database.Execute("INSERT INTO UmbCheckoutStripeShipping (Name, Value, \"Key\") VALUES ('Standard', 'shr_1', NULL)");
            _database.Execute("INSERT INTO UmbCheckoutStripeShipping (Name, Value, \"Key\") VALUES ('Express', 'shr_2', NULL)");

            _database.Migrate(c => new FillMissingShippingKeys(c), c => new AddKeyUniqueConstraint(c), c => new AddStripeIdUniqueConstraint(c));

            Assert.Equal(0, _database.Scalar<int>("SELECT COUNT(*) FROM UmbCheckoutStripeShipping WHERE \"Key\" IS NULL"));
            Assert.Equal(2, _database.Scalar<int>("SELECT COUNT(DISTINCT \"Key\") FROM UmbCheckoutStripeShipping"));
            var mapper = new UmbracoMapper(new MapDefinitionCollection(() => new IMapDefinition[] { new UmbCheckoutConfigurationMappingDefinition() }), Mock.Of<ICoreScopeProvider>(), NullLogger<UmbracoMapper>.Instance);
            var service = new StripeShippingRateDatabaseService(_database.ScopeProvider, mapper, NullLogger<StripeShippingRateDatabaseService>.Instance);
            var rates = (await service.GetShippingRates()).ToList();
            Assert.All(rates, r => Assert.NotEqual(Guid.Empty, r.Key));
            Assert.Equal("shr_1", (await service.GetShippingRate(rates.Single(r => r.Name == "Standard").Key))!.Value);
        }

        [Fact]
        public void A_rate_that_already_has_a_key_keeps_it()
        {
            _database.Migrate(c => new AddUmbCheckoutStripeShippingTable(c), c => new AddStripeShippingKey(c));
            var key = Guid.NewGuid();
            _database.Execute("INSERT INTO UmbCheckoutStripeShipping (Name, Value, \"Key\") VALUES ('Standard', 'shr_1', @0)", key);
            var before = _database.Scalar<string>("SELECT CAST(\"Key\" AS VARCHAR(50)) FROM UmbCheckoutStripeShipping");

            _database.Migrate(c => new FillMissingShippingKeys(c));

            Assert.Equal(before, _database.Scalar<string>("SELECT CAST(\"Key\" AS VARCHAR(50)) FROM UmbCheckoutStripeShipping"));
        }
    }

    /// <summary>The same cases against SQL Server in a container (needs Docker).</summary>
    [Trait("Category", "Integration")]
    [Collection(DatabaseCollection.Name)]
    public sealed class FillMissingKeysSqlServerTests : FillMissingKeysTests
    {
        public FillMissingKeysSqlServerTests(SqlServerContainerFixture sqlServer) : base(NPocoTestDatabase.SqlServer(sqlServer))
        {
        }
    }
}
