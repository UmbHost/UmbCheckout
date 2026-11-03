using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using UmbCheckout.Stripe.Notifications;
using UmbCheckout.Stripe.Persistence;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Shared.Notifications;
using UmbCheckout.Shared.Persistence;
using UmbCheckout.Testing;
using Umbraco.Cms.Core.Events;
using Xunit;
using SettingsModel = UmbCheckout.Stripe.Models.UmbCheckoutStripeSettings;

namespace UmbCheckout.Tests.Persistence;

[Trait("Category", "Integration")] // SQL Server cases need Docker
[Collection(DatabaseCollection.Name)]
public sealed class StripeSettingsServiceTests(SqlServerContainerFixture sqlServer)
{
    public static TheoryData<TestProvider> Providers => new() { TestProvider.Sqlite, TestProvider.SqlServer };

    private readonly SettingsCache<UmbCheckout.Stripe.Pocos.UmbCheckoutStripeSettings> _cache = new();

    private async Task<(TestDatabase<UmbCheckoutStripeDbContext> Database, StripeSettingsService Service, TestEventAggregator Events, TestEventMessagesFactory Messages)> Arrange(TestProvider provider)
    {
        var database = await TestDatabase.CreateAsync<UmbCheckoutStripeDbContext>(provider, sqlServer, o => new UmbCheckoutStripeDbContext(o), UmbCheckoutStripeDbContext.HistoryTable);
        await using (var db = database.CreateContext())
        {
            await db.Database.MigrateAsync();
        }

        var events = new TestEventAggregator();
        var messages = new TestEventMessagesFactory();
        var service = new StripeSettingsService(database.ScopeProvider, NullLogger<StripeSettingsService>.Instance,
            TestMapper.Create(new UmbCheckout.Stripe.Composers.UmbCheckoutConfigurationMappingDefinition()), TestLicenseService.None, events, messages, _cache, scopeAccessor: database.ScopeProvider.ScopeAccessor);
        return (database, service, events, messages);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_read_inside_an_unfinished_outer_scope_is_not_cached(TestProvider provider)
    {
        // Quality Phase 5 review: a rolled-back uSync import must not leave its uncommitted settings in the cache.
        var (database, service, _, _) = await Arrange(provider);
        await using var _db = database;

        using (database.ScopeProvider.CreateScope())
        {
            await service.UpdateStripeSettingsAsync(new SettingsModel { UseLiveApiDetails = true });
            Assert.True((await service.GetStripeSettingsAsync()).Required().UseLiveApiDetails);
        } // not completed: rolled back

        Assert.Null(await service.GetStripeSettingsAsync());
    }

    [Fact]
    public void The_Saved_handler_clears_the_cache_after_the_outermost_commit()
    {
        var cache = new SettingsCache<UmbCheckout.Stripe.Pocos.UmbCheckoutStripeSettings>();
        cache.TrySet(new UmbCheckout.Stripe.Pocos.UmbCheckoutStripeSettings(), cache.Generation);

        new UmbCheckout.Stripe.Cache.UmbCheckoutStripeSettingsSavedCacheHandler(cache).Handle(new OnStripeSettingsSavedNotification(new SettingsModel()));

        Assert.False(cache.TryGet(out _));
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task First_save_inserts_then_updates_the_single_row_and_notifies_once_per_save(TestProvider provider)
    {
        var (database, service, _, _) = await Arrange(provider);
        await using var _ = database;

        Assert.Null(await service.GetStripeSettingsAsync());
        Assert.True(await service.UpdateStripeSettingsAsync(new SettingsModel { CollectPhoneNumber = true, ShippingAllowedCountries = "GB" }));
        Assert.True(await service.UpdateStripeSettingsAsync(new SettingsModel { CollectPhoneNumber = false, ShippingAllowedCountries = "GB,US", AllowPromotionalCodes = true }));

        var saved = (await service.GetStripeSettingsAsync()).Required();
        Assert.False(saved.CollectPhoneNumber);
        Assert.True(saved.AllowPromotionalCodes);
        Assert.Equal("GB,US", saved.ShippingAllowedCountries);
        Assert.NotEqual(Guid.Empty, saved.Key);
        Assert.Equal(1, await database.ScalarAsync<int>("SELECT COUNT(*) FROM UmbCheckoutStripeSettings"));
        Assert.Equal(2, database.ScopeProvider.Notifications.Published.OfType<OnStripeSettingsSavedNotification>().Count());
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_legacy_row_with_a_null_key_reads_as_empty_and_gets_a_key_on_the_next_save(TestProvider provider)
    {
        var (database, service, _, _) = await Arrange(provider);
        await using var _ = database;
        await database.ExecuteSqlAsync("INSERT INTO UmbCheckoutStripeSettings (\"Key\", UseLiveApiDetails, ShippingAllowedCountries, CollectPhoneNumber, CollectPromotionalConsent, AllowPromotionalCodes, EnableAbandonedCartRecovery, AllowPromotionalCodesOnRecoveredCarts) VALUES (NULL, 0, NULL, 0, 0, 0, 0, 0)");

        var legacy = (await service.GetStripeSettingsAsync()).Required();
        Assert.Equal(Guid.Empty, legacy.Key);

        await service.UpdateStripeSettingsAsync(legacy);
        Assert.NotEqual(Guid.Empty, (await service.GetStripeSettingsAsync()).Required().Key);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_Saving_handler_changes_what_is_saved(TestProvider provider)
    {
        var (database, service, events, _) = await Arrange(provider);
        await using var _db = database;
        events.On<OnStripeSettingsSavingNotification>(n => n.Target.ShippingAllowedCountries = "GB");

        Assert.True(await service.UpdateStripeSettingsAsync(new SettingsModel { ShippingAllowedCountries = "GB,US" }));

        Assert.Equal("GB", (await service.GetStripeSettingsAsync()).Required().ShippingAllowedCountries);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_cancel_saves_nothing_and_keeps_the_message(TestProvider provider)
    {
        var (database, service, events, messages) = await Arrange(provider);
        await using var _db = database;
        events.On<OnStripeSettingsSavingNotification>(n => n.CancelOperation(new EventMessage("Stripe", "Live mode needs approval", EventMessageType.Error)));

        Assert.False(await service.UpdateStripeSettingsAsync(new SettingsModel { UseLiveApiDetails = true }));

        Assert.Equal(0, await database.ScalarAsync<int>("SELECT COUNT(*) FROM UmbCheckoutStripeSettings"));
        Assert.Empty(database.ScopeProvider.Notifications.Published.OfType<OnStripeSettingsSavedNotification>());
        Assert.Equal("Live mode needs approval", messages.Messages.CancellationMessage());
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task An_update_hands_the_handler_the_stored_settings(TestProvider provider)
    {
        var (database, service, events, _) = await Arrange(provider);
        await using var _db = database;
        await service.UpdateStripeSettingsAsync(new SettingsModel { ShippingAllowedCountries = "GB" });
        string? before = null;
        events.On<OnStripeSettingsSavingNotification>(n => before = n.Existing?.ShippingAllowedCountries);

        await service.UpdateStripeSettingsAsync(new SettingsModel { ShippingAllowedCountries = "US" });

        Assert.Equal("GB", before);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_Loaded_handler_changes_what_the_caller_gets_once_per_read(TestProvider provider)
    {
        var (database, service, events, _) = await Arrange(provider);
        await using var _db = database;
        await service.UpdateStripeSettingsAsync(new SettingsModel { CollectPhoneNumber = false });
        events.On<OnStripeSettingsLoadedNotification>(n => n.Target.CollectPhoneNumber = true);

        Assert.True((await service.GetStripeSettingsAsync()).Required().CollectPhoneNumber);
        Assert.Single(events.Published.OfType<OnStripeSettingsLoadedNotification>());
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_second_read_is_served_from_the_cache(TestProvider provider)
    {
        // Quality Phase 5: the settings were read from the database on every Stripe call.
        var (database, service, _, _) = await Arrange(provider);
        await using var _db = database;
        await service.UpdateStripeSettingsAsync(new SettingsModel { ShippingAllowedCountries = "GB" });
        await service.GetStripeSettingsAsync();
        await database.ExecuteSqlAsync("UPDATE UmbCheckoutStripeSettings SET ShippingAllowedCountries = 'US'");

        Assert.Equal("GB", (await service.GetStripeSettingsAsync()).Required().ShippingAllowedCountries);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_save_clears_the_cache_and_reads_never_share_an_instance(TestProvider provider)
    {
        var (database, service, events, _) = await Arrange(provider);
        await using var _db = database;
        await service.UpdateStripeSettingsAsync(new SettingsModel { ShippingAllowedCountries = "GB" });
        (await service.GetStripeSettingsAsync()).Required().ShippingAllowedCountries = "mutated by a caller";

        await service.UpdateStripeSettingsAsync(new SettingsModel { ShippingAllowedCountries = "FR" });
        var second = await service.GetStripeSettingsAsync();
        var third = await service.GetStripeSettingsAsync();

        Assert.Equal("FR", second.Required().ShippingAllowedCountries);
        Assert.NotSame(second, third);
        Assert.Equal(3, events.Published.OfType<OnStripeSettingsLoadedNotification>().Count());
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task The_cache_refresher_clears_the_cache_on_every_server(TestProvider provider)
    {
        var (database, service, _, _) = await Arrange(provider);
        await using var _db = database;
        await service.UpdateStripeSettingsAsync(new SettingsModel { ShippingAllowedCountries = "GB" });
        await service.GetStripeSettingsAsync();
        await database.ExecuteSqlAsync("UPDATE UmbCheckoutStripeSettings SET ShippingAllowedCountries = 'US'");

        new UmbCheckout.Stripe.Cache.UmbCheckoutStripeSettingsCacheRefresher(_cache).RefreshAll();

        Assert.Equal("US", (await service.GetStripeSettingsAsync()).Required().ShippingAllowedCountries);
    }
}