using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UmbCheckout.Stripe.Composers;
using UmbCheckout.Stripe.Migrations;
using UmbCheckout.Stripe.Models;
using UmbCheckout.Stripe.Notifications;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Tests.Infrastructure;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Scoping;
using Xunit;

namespace UmbCheckout.Tests.Persistence
{
    /// <summary>
    /// A shipping rate is updated by its Key, so an editor can change its Stripe ID (v13 looked it up by Stripe ID, didn't find it,
    /// and inserted a second row with the same Key: a 500). Real NPoco on SQLite.
    /// </summary>
    public class ShippingRateDatabaseServiceTests : IDisposable
    {
        private readonly NPocoTestDatabase _database;
        private readonly StripeShippingRateDatabaseService _service;

        public ShippingRateDatabaseServiceTests() : this(new NPocoTestDatabase())
        {
        }

        protected ShippingRateDatabaseServiceTests(NPocoTestDatabase database)
        {
            _database = database;
            _database.Migrate(c => new AddUmbCheckoutStripeShippingTable(c), c => new AddStripeShippingKey(c), c => new AddKeyUniqueConstraint(c), c => new AddStripeIdUniqueConstraint(c));
            var mapper = new UmbracoMapper(new MapDefinitionCollection(() => new IMapDefinition[] { new UmbCheckoutConfigurationMappingDefinition() }), Mock.Of<ICoreScopeProvider>(), NullLogger<UmbracoMapper>.Instance);
            _service = new StripeShippingRateDatabaseService(_database.ScopeProvider, mapper, NullLogger<StripeShippingRateDatabaseService>.Instance);
        }

        public void Dispose() => _database.Dispose();

        private int Rows => _database.Scalar<int>("SELECT COUNT(*) FROM UmbCheckoutStripeShipping");

        [Fact]
        public async Task Changing_the_Stripe_ID_updates_the_row_in_place()
        {
            var created = (await _service.CreateShippingRate(new ShippingRate { Name = "Standard", Value = "shr_old" })).Required();

            var updated = await _service.UpdateShippingRate(new ShippingRate { Key = created.Key, Name = "Standard", Value = "shr_new" });

            Assert.NotNull(updated);
            Assert.Equal("shr_new", updated.Required().Value);
            Assert.Equal(created.Key, updated.Key);
            Assert.Equal(1, Rows);
        }

        [Fact]
        public async Task Changing_the_Stripe_ID_to_another_rates_ID_is_refused()
        {
            var first = (await _service.CreateShippingRate(new ShippingRate { Name = "One", Value = "shr_1" })).Required();
            await _service.CreateShippingRate(new ShippingRate { Name = "Two", Value = "shr_2" });

            Assert.Null(await _service.UpdateShippingRate(new ShippingRate { Key = first.Key, Name = "One", Value = "shr_2" }));
            Assert.Equal("shr_1", (await _service.GetShippingRate(first.Key)).Required().Value);
            Assert.Equal(2, Rows);
        }

        [Fact]
        public async Task A_uSync_import_with_an_unknown_key_adopts_the_row_by_Stripe_ID_or_inserts()
        {
            var existing = (await _service.CreateShippingRate(new ShippingRate { Name = "Old name", Value = "shr_1" })).Required();

            // Another environment's key for the same Stripe rate: the row is adopted by Stripe ID and keeps its key.
            var adopted = (await _service.UpdateShippingRate(new ShippingRate { Key = Guid.NewGuid(), Name = "New name", Value = "shr_1" })).Required();
            // A rate this site doesn't have yet: inserted with the imported key.
            var importedKey = Guid.NewGuid();
            var inserted = await _service.UpdateShippingRate(new ShippingRate { Key = importedKey, Name = "Express", Value = "shr_2" });

            Assert.Equal(existing.Key, adopted.Key);
            Assert.Equal("New name", adopted.Name);
            Assert.Equal(importedKey, inserted.Required().Key);
            Assert.Equal(2, Rows);
        }

        [Fact]
        public async Task A_saved_rate_publishes_its_saved_notification()
        {
            var created = (await _service.CreateShippingRate(new ShippingRate { Name = "Standard", Value = "shr_old" })).Required();
            _database.Published.Clear();

            await _service.UpdateShippingRate(new ShippingRate { Key = created.Key, Name = "Standard", Value = "shr_new" });

            Assert.Equal("shr_new", Assert.IsType<OnShippingRateSavedNotification>(Assert.Single(_database.Published)).ShippingRate?.Value);
        }
    }
}
