using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NPoco.Linq;
using UmbCheckout.Core.Cache;
using UmbCheckout.Core.Services;
using UmbCheckout.Shared.Notifications.Configuration;
using UmbCheckout.Shared.Persistence;
using UmbCheckout.Stripe.Cache;
using UmbCheckout.Stripe.Migrations;
using UmbCheckout.Stripe.Notifications;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Testing;
using UmbCheckout.Tests.Infrastructure;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Persistence;
using Xunit;
using ConfigurationModel = UmbCheckout.Shared.Models.UmbCheckoutConfiguration;
using ConfigurationPoco = UmbCheckout.Core.Pocos.UmbCheckoutConfiguration;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;
using IScopeAccessor = Umbraco.Cms.Infrastructure.Scoping.IScopeAccessor;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;
using StripeSettingsModel = UmbCheckout.Stripe.Models.UmbCheckoutStripeSettings;
using StripeSettingsPoco = UmbCheckout.Stripe.Pocos.UmbCheckoutStripeSettings;

namespace UmbCheckout.Tests.Persistence
{
    /// <summary>
    /// Group 6: the configuration and Stripe settings are read from the database once per process, not on every shop request. A save
    /// clears the cache here and, once committed, on every load-balanced server (the Saved handler tells Umbraco's distributed cache,
    /// which calls the cache refresher on each server). A read can't put an older row back after a save, and a read inside someone
    /// else's scope (possibly uncommitted) isn't cached.
    /// </summary>
    public class SettingsCacheTests : IDisposable
    {
        private readonly List<NPocoTestDatabase> _databases = new();

        public void Dispose() => _databases.ForEach(d => d.Dispose());

        /// <summary>A new database for the engine under test (SQLite; the SQL Server cases override this).</summary>
        protected virtual NPocoTestDatabase NewDatabase() => new();

        /// <summary>A database holding one settings row, counting reads; scopes can be made "ambient" (someone else's).</summary>
        private sealed class FakeStore<TPoco> where TPoco : class
        {
            public TPoco? Row { get; set; }

            public int Reads { get; private set; }

            public Mock<IUmbracoDatabase> Database { get; } = new();

            public Mock<IScopeAccessor> ScopeAccessor { get; } = new();

            public List<INotification> Published { get; } = new();

            public Mock<IScopeProvider> ScopeProvider { get; } = new();

            public Mock<ICoreScopeProvider> CoreScopeProvider { get; } = new();

            /// <summary>What SingleOrDefault gives when there is no row: Moq answers a query nobody set up with a finished task with no result.</summary>
            private static readonly Task<TPoco> NoRow = new Mock<IAsyncQueryProviderWithIncludes<TPoco>>().Object.SingleOrDefault();

            public FakeStore()
            {
                var query = new Mock<IAsyncQueryProviderWithIncludes<TPoco>>();
                query.Setup(q => q.SingleOrDefault()).Returns(() =>
                {
                    Reads++;
                    return Row is { } row ? Task.FromResult(row) : NoRow;
                });
                Database.Setup(d => d.QueryAsync<TPoco>()).Returns(query.Object);
                Database.Setup(d => d.UpdateAsync(It.IsAny<TPoco>())).Callback<object>(p => Row = (TPoco)p).ReturnsAsync(1);
                Database.Setup(d => d.InsertAsync(It.IsAny<TPoco>())).Callback<TPoco>(p => Row = p).ReturnsAsync(1);

                var notifications = new Mock<IScopedNotificationPublisher>();
                notifications.Setup(n => n.Publish(It.IsAny<INotification>())).Callback<INotification>(Published.Add);
                var scope = new Mock<IScope>();
                scope.SetupGet(s => s.Database).Returns(Database.Object);
                scope.SetupGet(s => s.Notifications).Returns(notifications.Object);
                ScopeProvider.Setup(p => p.CreateScope(It.IsAny<IsolationLevel>(), It.IsAny<RepositoryCacheMode>(), It.IsAny<IEventDispatcher?>(),
                        It.IsAny<IScopedNotificationPublisher?>(), It.IsAny<bool?>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Returns(scope.Object);
                CoreScopeProvider.Setup(p => p.CreateCoreScope(It.IsAny<IsolationLevel>(), It.IsAny<RepositoryCacheMode>(), It.IsAny<IEventDispatcher?>(),
                        It.IsAny<IScopedNotificationPublisher?>(), It.IsAny<bool?>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Returns(scope.Object);
            }

            /// <summary>Reads from now on run inside someone else's scope (a uSync import, a save).</summary>
            public void InsideAnotherScope() => ScopeAccessor.SetupGet(a => a.AmbientScope).Returns(Mock.Of<IScope>());
        }

        private static IUmbracoMapper ConfigurationMapper()
        {
            var mapper = new Mock<IUmbracoMapper>();
            mapper.Setup(m => m.Map<ConfigurationPoco, ConfigurationModel>(It.IsAny<ConfigurationPoco>()))
                .Returns<object?>(p => p is ConfigurationPoco poco ? new ConfigurationModel { Id = poco.Id, Key = poco.Key, CurrencyCode = poco.CurrencyCode } : null);
            mapper.Setup(m => m.Map<ConfigurationModel, ConfigurationPoco>(It.IsAny<ConfigurationModel>()))
                .Returns<object?>(c => c is ConfigurationModel model ? new ConfigurationPoco { CurrencyCode = model.CurrencyCode } : null);
            return mapper.Object;
        }

        private static (ConfigurationService Service, FakeStore<ConfigurationPoco> Store, SettingsCache<ConfigurationPoco> Cache) Configuration(string currency = "GBP")
        {
            var store = new FakeStore<ConfigurationPoco> { Row = new ConfigurationPoco { Id = 1, CurrencyCode = currency } };
            var cache = new SettingsCache<ConfigurationPoco>();
            var service = new ConfigurationService(store.ScopeProvider.Object, NullLogger<ConfigurationService>.Instance, ConfigurationMapper(), store.CoreScopeProvider.Object, null, cache, store.ScopeAccessor.Object);
            return (service, store, cache);
        }

        /// <summary>
        /// The Stripe settings on real NPoco and SQLite (Moq can't proxy queries over the Stripe package's internal POCO). A cached
        /// read is seen by changing the row behind the service: a cached service still answers with the row it read.
        /// </summary>
        private (StripeSettingsService Service, NPocoTestDatabase Database, SettingsCache<StripeSettingsPoco> Cache, Mock<IScopeAccessor> ScopeAccessor) StripeSettings(bool? live = false)
        {
            var database = NewDatabase();
            _databases.Add(database);
            database.Migrate(c => new AddUmbCheckoutStripeSettingsTable(c), c => new AddStripeShippingAllowedCountries(c), c => new AddStripeSettingsCollectPhoneNumber(c),
                c => new AddStripeSettingsCollectPromotionalConsent(c), c => new AddStripeSettingsAllowPromotionalCodes(c), c => new AddStripeSettingsAbandonedCarts(c));
            if (live.HasValue)
            {
                database.Database.Insert(new StripeSettingsPoco { UseLiveApiDetails = live.Value });
            }

            var mapper = new UmbracoMapper(new MapDefinitionCollection(() => new IMapDefinition[] { new UmbCheckout.Stripe.Composers.UmbCheckoutConfigurationMappingDefinition() }),
                Mock.Of<ICoreScopeProvider>(), NullLogger<UmbracoMapper>.Instance);
            var cache = new SettingsCache<StripeSettingsPoco>();
            var scopeAccessor = new Mock<IScopeAccessor>();
            var service = new StripeSettingsService(database.ScopeProvider, NullLogger<StripeSettingsService>.Instance, mapper, database.ScopeProvider, null, cache, scopeAccessor.Object);
            return (service, database, cache, scopeAccessor);
        }

        private static void DeleteTheRow(NPocoTestDatabase database) => database.Execute("DELETE FROM UmbCheckoutStripeSettings");

        [Fact]
        public async Task The_configuration_is_read_from_the_database_once()
        {
            var (service, store, _) = Configuration();

            await service.GetConfiguration();
            await service.GetConfiguration();

            Assert.Equal(1, store.Reads);
        }

        [Fact]
        public async Task The_Stripe_settings_are_read_from_the_database_once()
        {
            var (service, database, _, _) = StripeSettings(live: true);

            await service.GetStripeSettings();
            DeleteTheRow(database);

            Assert.True((await service.GetStripeSettings()).Required().UseLiveApiDetails);
        }

        [Fact]
        public async Task Every_configuration_read_gets_its_own_instance()
        {
            var (service, _, _) = Configuration("GBP");

            var first = await service.GetConfiguration();
            first.Required().CurrencyCode = "XXX";

            Assert.NotSame(first, await service.GetConfiguration());
            Assert.Equal("GBP", (await service.GetConfiguration()).Required().CurrencyCode);
        }

        [Fact]
        public async Task Every_Stripe_settings_read_gets_its_own_instance()
        {
            var (service, _, _, _) = StripeSettings(live: false);

            var first = await service.GetStripeSettings();
            first.Required().UseLiveApiDetails = true;

            Assert.False((await service.GetStripeSettings()).Required().UseLiveApiDetails);
        }

        [Fact]
        public async Task A_saved_configuration_is_read_back()
        {
            var (service, _, _) = Configuration("GBP");
            await service.GetConfiguration();

            Assert.True(await service.UpdateConfiguration(new ConfigurationModel { CurrencyCode = "EUR" }));

            Assert.Equal("EUR", (await service.GetConfiguration()).Required().CurrencyCode);
        }

        [Fact]
        public async Task Saved_Stripe_settings_are_read_back()
        {
            var (service, _, _, _) = StripeSettings(live: false);
            await service.GetStripeSettings();

            Assert.True(await service.UpdateStripeSettings(new StripeSettingsModel { UseLiveApiDetails = true }));

            Assert.True((await service.GetStripeSettings()).Required().UseLiveApiDetails);
        }

        [Fact]
        public async Task The_saved_notification_carries_the_saved_configuration_not_the_cached_one()
        {
            var (service, store, _) = Configuration("GBP");
            await service.GetConfiguration();

            await service.UpdateConfiguration(new ConfigurationModel { CurrencyCode = "EUR" });

            Assert.Equal("EUR", Assert.Single(store.Published.OfType<OnConfigurationSavedNotification>()).Configuration.Required().CurrencyCode);
        }

        [Fact]
        public async Task The_saved_notification_carries_the_saved_Stripe_settings_not_the_cached_ones()
        {
            var (service, database, _, _) = StripeSettings(live: false);
            await service.GetStripeSettings();

            await service.UpdateStripeSettings(new StripeSettingsModel { UseLiveApiDetails = true });

            Assert.True(Assert.Single(database.Published.OfType<OnStripeSettingsSavedNotification>()).StripeSettings.Required().UseLiveApiDetails);
        }

        [Fact]
        public async Task The_first_save_publishes_saved_too()
        {
            // So uSync writes its file and every server's cache is cleared (the first save used to publish nothing).
            var (configuration, configurationStore, _) = Configuration();
            configurationStore.Row = null;
            var (stripeSettings, database, _, _) = StripeSettings(live: null);

            await configuration.UpdateConfiguration(new ConfigurationModel { CurrencyCode = "EUR" });
            await stripeSettings.UpdateStripeSettings(new StripeSettingsModel { UseLiveApiDetails = true });

            Assert.Single(configurationStore.Published.OfType<OnConfigurationSavedNotification>());
            Assert.True(Assert.Single(database.Published.OfType<OnStripeSettingsSavedNotification>()).StripeSettings.Required().UseLiveApiDetails);
            Assert.Equal(1, database.Scalar<int>("SELECT COUNT(*) FROM UmbCheckoutStripeSettings"));
        }

        [Fact]
        public async Task A_save_updates_the_stored_row_even_when_this_servers_cache_says_none_is_saved()
        {
            // Another server created the row; this server's cache still says "none saved" (the refresh hasn't arrived): no second row.
            var (service, store, _) = Configuration();
            store.Row = null;
            await service.GetConfiguration();
            store.Row = new ConfigurationPoco { Id = 7, CurrencyCode = "GBP" };

            await service.UpdateConfiguration(new ConfigurationModel { CurrencyCode = "EUR" });

            store.Database.Verify(d => d.InsertAsync(It.IsAny<ConfigurationPoco>()), Times.Never);
            Assert.Equal(7, store.Row.Id);
        }

        [Fact]
        public async Task A_read_inside_another_scope_is_not_cached()
        {
            // The row may be uncommitted (a uSync import, a save) and could roll back: it is read but never cached.
            var (configuration, configurationStore, _) = Configuration();
            configurationStore.InsideAnotherScope();
            var (stripeSettings, database, _, scopeAccessor) = StripeSettings(live: true);
            scopeAccessor.SetupGet(a => a.AmbientScope).Returns(Mock.Of<IScope>());

            await configuration.GetConfiguration();
            await configuration.GetConfiguration();
            await stripeSettings.GetStripeSettings();
            DeleteTheRow(database);

            Assert.Equal(2, configurationStore.Reads);
            Assert.Null(await stripeSettings.GetStripeSettings());
        }

        [Fact]
        public async Task A_read_inside_another_scope_still_uses_the_cache()
        {
            var (service, store, _) = Configuration();
            await service.GetConfiguration();
            store.InsideAnotherScope();

            await service.GetConfiguration();

            Assert.Equal(1, store.Reads);
        }

        [Fact]
        public void A_read_that_started_before_a_clear_is_not_cached()
        {
            var cache = new SettingsCache<ConfigurationPoco>();
            var generation = cache.Generation;
            cache.Clear(); // a save committed while the read was querying

            cache.TrySet(new ConfigurationPoco { CurrencyCode = "OLD" }, generation);

            Assert.False(cache.TryGet(out _));
        }

        [Fact]
        public void None_saved_yet_is_cached_too()
        {
            var cache = new SettingsCache<ConfigurationPoco>();

            cache.TrySet(null, cache.Generation);

            Assert.True(cache.TryGet(out var value));
            Assert.Null(value);
        }

        [Fact]
        public async Task The_cache_refreshers_clear_the_cache()
        {
            var (configuration, configurationStore, configurationCache) = Configuration();
            var (stripeSettings, database, stripeCache, _) = StripeSettings(live: false);
            await configuration.GetConfiguration();
            await stripeSettings.GetStripeSettings();
            database.Execute("UPDATE UmbCheckoutStripeSettings SET UseLiveApiDetails = 1");

            // Another server saved: Umbraco's distributed cache calls each refresher on this one.
            new UmbCheckoutConfigurationCacheRefresher(configurationCache).RefreshAll();
            new UmbCheckoutStripeSettingsCacheRefresher(stripeCache).RefreshAll();
            await configuration.GetConfiguration();

            Assert.Equal(2, configurationStore.Reads);
            Assert.True((await stripeSettings.GetStripeSettings()).Required().UseLiveApiDetails);
        }

        [Fact]
        public void The_saved_handlers_clear_this_server_and_tell_the_others()
        {
            var configurationCache = new SettingsCache<ConfigurationPoco>();
            var stripeCache = new SettingsCache<StripeSettingsPoco>();
            configurationCache.TrySet(new ConfigurationPoco(), configurationCache.Generation);
            stripeCache.TrySet(new StripeSettingsPoco(), stripeCache.Generation);
            var configurationRefresher = new UmbCheckoutConfigurationCacheRefresher(configurationCache);
            var stripeRefresher = new UmbCheckoutStripeSettingsCacheRefresher(stripeCache);
            var messenger = new Mock<IServerMessenger>();
            var distributedCache = new DistributedCache(messenger.Object, new CacheRefresherCollection(() => new ICacheRefresher[] { configurationRefresher, stripeRefresher }));

            new UmbCheckoutConfigurationSavedCacheHandler(configurationCache, distributedCache).Handle(new OnConfigurationSavedNotification(null));
            new UmbCheckoutStripeSettingsSavedCacheHandler(stripeCache, distributedCache).Handle(new OnStripeSettingsSavedNotification(null));

            Assert.False(configurationCache.TryGet(out _));
            Assert.False(stripeCache.TryGet(out _));
            messenger.Verify(m => m.QueueRefreshAll(configurationRefresher), Times.Once);
            messenger.Verify(m => m.QueueRefreshAll(stripeRefresher), Times.Once);
        }

        [Fact]
        public void The_refreshers_have_distinct_ids()
            => Assert.NotEqual(UmbCheckoutConfigurationCacheRefresher.UniqueId, UmbCheckoutStripeSettingsCacheRefresher.UniqueId);

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void The_composers_register_one_cache_its_saved_handler_and_its_refresher(bool stripe)
        {
            var services = new ServiceCollection();
            var refreshers = new CacheRefresherCollectionBuilder();
            var builder = new Mock<IUmbracoBuilder>();
            builder.SetupGet(b => b.Services).Returns(services);
            builder.Setup(b => b.WithCollectionBuilder<CacheRefresherCollectionBuilder>()).Returns(refreshers);

            if (stripe)
            {
                new UmbCheckout.Stripe.Composers.SettingsCacheComposer().Compose(builder.Object);
                Assert.Contains(services, d => d.ServiceType == typeof(SettingsCache<StripeSettingsPoco>) && d.Lifetime == ServiceLifetime.Singleton);
                Assert.Contains(services, d => d.ServiceType == typeof(INotificationHandler<OnStripeSettingsSavedNotification>) && d.ImplementationType == typeof(UmbCheckoutStripeSettingsSavedCacheHandler));
                Assert.True(refreshers.Has<UmbCheckoutStripeSettingsCacheRefresher>());
            }
            else
            {
                new UmbCheckout.Core.Composers.SettingsCacheComposer().Compose(builder.Object);
                Assert.Contains(services, d => d.ServiceType == typeof(SettingsCache<ConfigurationPoco>) && d.Lifetime == ServiceLifetime.Singleton);
                Assert.Contains(services, d => d.ServiceType == typeof(INotificationHandler<OnConfigurationSavedNotification>) && d.ImplementationType == typeof(UmbCheckoutConfigurationSavedCacheHandler));
                Assert.True(refreshers.Has<UmbCheckoutConfigurationCacheRefresher>());
            }
        }
    }

    /// <summary>The same cases against SQL Server in a container (needs Docker).</summary>
    [Trait("Category", "Integration")]
    [Collection(DatabaseCollection.Name)]
    public sealed class SettingsCacheSqlServerTests : SettingsCacheTests
    {
        private readonly SqlServerContainerFixture _sqlServer;

        public SettingsCacheSqlServerTests(SqlServerContainerFixture sqlServer) => _sqlServer = sqlServer;

        protected override NPocoTestDatabase NewDatabase() => NPocoTestDatabase.SqlServer(_sqlServer);
    }
}
