using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using UmbCheckout.Core.Persistence;
using UmbCheckout.Core.Services;
using UmbCheckout.Shared.Models;
using UmbCheckout.Shared.Notifications;
using UmbCheckout.Shared.Persistence;
using UmbCheckout.Shared.Notifications.Configuration;
using UmbCheckout.Testing;
using Umbraco.Cms.Core.Events;
using Xunit;
using ConfigurationModel = UmbCheckout.Shared.Models.UmbCheckoutConfiguration;

namespace UmbCheckout.Tests.Persistence;

[Collection(DatabaseCollection.Name)]
public sealed class ConfigurationServiceTests(SqlServerContainerFixture sqlServer)
{
    public static TheoryData<TestProvider> Providers => new() { TestProvider.Sqlite, TestProvider.SqlServer };

    private SettingsCache<UmbCheckout.Core.Pocos.UmbCheckoutConfiguration> _cache = new();

    private async Task<(TestDatabase<UmbCheckoutDbContext> Database, ConfigurationService Service, TestEventAggregator Events, TestEventMessagesFactory Messages)> Arrange(TestProvider provider)
    {
        var database = await TestDatabase.CreateAsync<UmbCheckoutDbContext>(provider, sqlServer, o => new UmbCheckoutDbContext(o), UmbCheckoutDbContext.HistoryTable);
        await using (var db = database.CreateContext())
        {
            await db.Database.MigrateAsync();
        }

        var events = new TestEventAggregator();
        var messages = new TestEventMessagesFactory();
        var service = new ConfigurationService(database.ScopeProvider, NullLogger<ConfigurationService>.Instance,
            TestMapper.Create(new UmbCheckout.Core.Composers.UmbCheckoutConfigurationMappingDefinition()), TestLicenseService.Create(), events, messages, _cache, scopeAccessor: database.ScopeProvider.ScopeAccessor);
        return (database, service, events, messages);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_read_inside_an_unfinished_outer_scope_is_not_cached(TestProvider provider)
    {
        // Quality Phase 5 review: uSync imports inside its own outer scope. A read there sees the uncommitted row; if the import
        // rolls back, the cache must not keep serving it.
        var (database, service, _, _) = await Arrange(provider);
        await using var _db = database;

        using (database.ScopeProvider.CreateScope())
        {
            await service.UpdateConfigurationAsync(new ConfigurationModel { SuccessPageUrl = Page("/a"), CancelPageUrl = Page("/b"), CurrencyCode = "GBP" });
            Assert.Equal("GBP", (await service.GetConfigurationAsync())!.CurrencyCode);
        } // not completed: rolled back

        Assert.Null(await service.GetConfigurationAsync());
    }

    [Fact]
    public void The_Saved_handler_clears_the_cache_after_the_outermost_commit()
    {
        // Umbraco publishes Saved when the outermost scope has committed, so a read that cached the old row in between is dropped.
        var cache = new SettingsCache<UmbCheckout.Core.Pocos.UmbCheckoutConfiguration>();
        cache.TrySet(new UmbCheckout.Core.Pocos.UmbCheckoutConfiguration { CurrencyCode = "old" }, cache.Generation);

        new UmbCheckout.Core.Cache.UmbCheckoutConfigurationSavedCacheHandler(cache).Handle(new OnConfigurationSavedNotification(new ConfigurationModel()));

        Assert.False(cache.TryGet(out _));
    }

    private static MultiUrlPicker[] Page(string url) => [new MultiUrlPicker { Name = url, Url = url }];

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task First_save_inserts_then_updates_the_single_row_and_notifies_once_per_save(TestProvider provider)
    {
        var (database, service, _, _) = await Arrange(provider);
        await using var _ = database;

        Assert.Null(await service.GetConfigurationAsync());
        Assert.True(await service.UpdateConfigurationAsync(new ConfigurationModel { SuccessPageUrl = Page("/a"), CancelPageUrl = Page("/b"), CurrencyCode = "GBP", StoreBasketInDatabase = true }));
        Assert.True(await service.UpdateConfigurationAsync(new ConfigurationModel { SuccessPageUrl = Page("/c"), CancelPageUrl = Page("/b"), CurrencyCode = "GBP", StoreBasketInDatabase = true }));

        var saved = await service.GetConfigurationAsync();
        Assert.Equal("/c", saved!.SuccessPageUrl.Single().Url);
        Assert.NotEqual(Guid.Empty, saved.Key);
        Assert.True(saved.StoreBasketInDatabase);
        Assert.Equal(1, await database.ScalarAsync<int>("SELECT COUNT(*) FROM UmbCheckoutConfiguration"));
        Assert.Equal(2, database.ScopeProvider.Notifications.Published.OfType<OnConfigurationSavedNotification>().Count());
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_legacy_row_with_a_null_key_reads_as_empty_and_gets_a_key_on_the_next_save(TestProvider provider)
    {
        var (database, service, _, _) = await Arrange(provider);
        await using var _ = database;
        await database.ExecuteSqlAsync("INSERT INTO UmbCheckoutConfiguration (SuccessPageUrl, CancelPageUrl, StoreBasketInCookie, StoreBasketInDatabase, BasketInDatabaseExpiry, BasketInCookieExpiry, EnableShipping, \"Key\", CurrencyCode) VALUES ('[]', '[]', 0, 0, 30, 30, 0, NULL, NULL)");

        var legacy = await service.GetConfigurationAsync();
        Assert.Equal(Guid.Empty, legacy!.Key);
        Assert.Equal(string.Empty, legacy.CurrencyCode);

        await service.UpdateConfigurationAsync(legacy);
        Assert.NotEqual(Guid.Empty, (await service.GetConfigurationAsync())!.Key);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Basket_storage_options_are_stored_as_given(TestProvider provider)
    {
        // Whether they apply is the licensed add-on's rule, not the open service's
        var (database, service, _, _) = await Arrange(provider);
        await using var _ = database;

        await service.UpdateConfigurationAsync(new ConfigurationModel { SuccessPageUrl = Page("/a"), CancelPageUrl = Page("/b"), StoreBasketInCookie = true, StoreBasketInDatabase = true });

        var saved = await service.GetConfigurationAsync();
        Assert.True(saved!.StoreBasketInCookie);
        Assert.True(saved.StoreBasketInDatabase);
        await using var db = database.CreateContext();
        var row = await db.Configurations.SingleAsync();
        Assert.True(row.StoreBasketInCookie);
        Assert.True(row.StoreBasketInDatabase);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_Saving_handler_changes_what_is_saved(TestProvider provider)
    {
        var (database, service, events, _) = await Arrange(provider);
        await using var _db = database;
        events.On<OnConfigurationSavingNotification>(n => n.Target.CurrencyCode = "EUR");

        Assert.True(await service.UpdateConfigurationAsync(new ConfigurationModel { SuccessPageUrl = Page("/a"), CancelPageUrl = Page("/b"), CurrencyCode = "GBP" }));

        Assert.Equal("EUR", (await service.GetConfigurationAsync())!.CurrencyCode);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_cancel_saves_nothing_and_keeps_the_message(TestProvider provider)
    {
        var (database, service, events, messages) = await Arrange(provider);
        await using var _db = database;
        events.On<OnConfigurationSavingNotification>(n =>
        {
            n.Target.CurrencyCode = "EUR";
            n.CancelOperation(new EventMessage("Configuration", "Currency changes are frozen", EventMessageType.Error));
        });

        Assert.False(await service.UpdateConfigurationAsync(new ConfigurationModel { SuccessPageUrl = Page("/a"), CancelPageUrl = Page("/b"), CurrencyCode = "GBP" }));

        Assert.Equal(0, await database.ScalarAsync<int>("SELECT COUNT(*) FROM UmbCheckoutConfiguration"));
        Assert.Empty(database.ScopeProvider.Notifications.Published.OfType<OnConfigurationSavedNotification>());
        Assert.Equal("Currency changes are frozen", messages.Messages.CancellationMessage());
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_cancel_without_a_message_saves_nothing_and_still_reads_as_a_cancel(TestProvider provider)
    {
        var (database, service, events, messages) = await Arrange(provider);
        await using var _db = database;
        events.On<OnConfigurationSavingNotification>(n => n.Cancel = true);

        Assert.False(await service.UpdateConfigurationAsync(new ConfigurationModel { SuccessPageUrl = Page("/a"), CancelPageUrl = Page("/b"), CurrencyCode = "GBP" }));

        Assert.Equal(0, await database.ScalarAsync<int>("SELECT COUNT(*) FROM UmbCheckoutConfiguration"));
        Assert.Equal(EventMessagesExtensions.DefaultCancellationMessage, messages.Messages.CancellationMessage());
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task An_update_hands_the_handler_the_stored_configuration(TestProvider provider)
    {
        var (database, service, events, _) = await Arrange(provider);
        await using var _db = database;
        await service.UpdateConfigurationAsync(new ConfigurationModel { SuccessPageUrl = Page("/a"), CancelPageUrl = Page("/b"), CurrencyCode = "GBP" });
        string? before = null;
        events.On<OnConfigurationSavingNotification>(n => before = n.Existing?.CurrencyCode);

        await service.UpdateConfigurationAsync(new ConfigurationModel { SuccessPageUrl = Page("/a"), CancelPageUrl = Page("/b"), CurrencyCode = "USD" });

        Assert.Equal("GBP", before);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_Loaded_handler_changes_what_the_caller_gets_once_per_read(TestProvider provider)
    {
        var (database, service, events, _) = await Arrange(provider);
        await using var _db = database;
        await service.UpdateConfigurationAsync(new ConfigurationModel { SuccessPageUrl = Page("/a"), CancelPageUrl = Page("/b"), CurrencyCode = "GBP" });
        events.On<OnConfigurationLoadedNotification>(n => n.Target.CurrencyCode = "JPY");

        var loaded = await service.GetConfigurationAsync();

        Assert.Equal("JPY", loaded!.CurrencyCode);
        Assert.Single(events.Published.OfType<OnConfigurationLoadedNotification>());
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Nothing_to_load_publishes_no_Loaded(TestProvider provider)
    {
        var (database, service, events, _) = await Arrange(provider);
        await using var _db = database;

        Assert.Null(await service.GetConfigurationAsync());
        Assert.Empty(events.Published.OfType<OnConfigurationLoadedNotification>());
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_second_read_is_served_from_the_cache(TestProvider provider)
    {
        // Quality Phase 5: configuration was read from the database several times per shop request.
        var (database, service, _, _) = await Arrange(provider);
        await using var _db = database;
        await service.UpdateConfigurationAsync(new ConfigurationModel { SuccessPageUrl = Page("/a"), CancelPageUrl = Page("/b"), CurrencyCode = "GBP" });
        await service.GetConfigurationAsync();
        await database.ExecuteSqlAsync("UPDATE UmbCheckoutConfiguration SET CurrencyCode = 'USD'");

        Assert.Equal("GBP", (await service.GetConfigurationAsync())!.CurrencyCode);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_save_clears_the_cache_and_reads_never_share_an_instance(TestProvider provider)
    {
        var (database, service, events, _) = await Arrange(provider);
        await using var _db = database;
        await service.UpdateConfigurationAsync(new ConfigurationModel { SuccessPageUrl = Page("/a"), CancelPageUrl = Page("/b"), CurrencyCode = "GBP" });
        var first = await service.GetConfigurationAsync();
        first!.CurrencyCode = "mutated by a caller";

        await service.UpdateConfigurationAsync(new ConfigurationModel { SuccessPageUrl = Page("/a"), CancelPageUrl = Page("/b"), CurrencyCode = "EUR" });
        var second = await service.GetConfigurationAsync();
        var third = await service.GetConfigurationAsync();

        Assert.Equal("EUR", second!.CurrencyCode);
        Assert.NotSame(second, third);
        Assert.Equal(3, events.Published.OfType<OnConfigurationLoadedNotification>().Count()); // Loaded still runs on every read
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task The_cache_refresher_clears_the_cache_on_every_server(TestProvider provider)
    {
        // Load balancing: another server saved; Umbraco's distributed cache calls this refresher here.
        var (database, service, _, _) = await Arrange(provider);
        await using var _db = database;
        await service.UpdateConfigurationAsync(new ConfigurationModel { SuccessPageUrl = Page("/a"), CancelPageUrl = Page("/b"), CurrencyCode = "GBP" });
        await service.GetConfigurationAsync();
        await database.ExecuteSqlAsync("UPDATE UmbCheckoutConfiguration SET CurrencyCode = 'USD'");

        new UmbCheckout.Core.Cache.UmbCheckoutConfigurationCacheRefresher(_cache).RefreshAll();

        Assert.Equal("USD", (await service.GetConfigurationAsync())!.CurrencyCode);
    }
}