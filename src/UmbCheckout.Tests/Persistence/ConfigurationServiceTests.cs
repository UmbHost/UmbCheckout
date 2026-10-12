using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using UmbCheckout.Core.Persistence;
using UmbCheckout.Core.Services;
using UmbCheckout.Shared.Models;
using UmbCheckout.Shared.Notifications.Configuration;
using UmbCheckout.Testing;
using Xunit;
using ConfigurationModel = UmbCheckout.Shared.Models.UmbCheckoutConfiguration;

namespace UmbCheckout.Tests.Persistence;

[Collection(DatabaseCollection.Name)]
public sealed class ConfigurationServiceTests(SqlServerContainerFixture sqlServer)
{
    public static TheoryData<TestProvider> Providers => new() { TestProvider.Sqlite, TestProvider.SqlServer };

    private async Task<(TestDatabase<UmbCheckoutDbContext> Database, ConfigurationService Service)> Arrange(TestProvider provider)
    {
        var database = await TestDatabase.CreateAsync<UmbCheckoutDbContext>(provider, sqlServer, o => new UmbCheckoutDbContext(o), UmbCheckoutDbContext.HistoryTable);
        await using (var db = database.CreateContext())
        {
            await db.Database.MigrateAsync();
        }

        var service = new ConfigurationService(database.ScopeProvider, NullLogger<ConfigurationService>.Instance,
            TestMapper.Create(new UmbCheckout.Core.Composers.UmbCheckoutConfigurationMappingDefinition()), TestLicenseService.Create());
        return (database, service);
    }

    private static MultiUrlPicker[] Page(string url) => [new MultiUrlPicker { Name = url, Url = url }];

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task First_save_inserts_then_updates_the_single_row_and_notifies_once_per_save(TestProvider provider)
    {
        var (database, service) = await Arrange(provider);
        await using var _ = database;
        using var licensed = LicenseState.Set(true);

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
        var (database, service) = await Arrange(provider);
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
        var (database, service) = await Arrange(provider);
        await using var _ = database;
        using var unlicensed = LicenseState.Set(false);

        await service.UpdateConfigurationAsync(new ConfigurationModel { SuccessPageUrl = Page("/a"), CancelPageUrl = Page("/b"), StoreBasketInCookie = true, StoreBasketInDatabase = true });

        var saved = await service.GetConfigurationAsync();
        Assert.True(saved!.StoreBasketInCookie);
        Assert.True(saved.StoreBasketInDatabase);
        await using var db = database.CreateContext();
        var row = await db.Configurations.SingleAsync();
        Assert.True(row.StoreBasketInCookie);
        Assert.True(row.StoreBasketInDatabase);
    }
}
