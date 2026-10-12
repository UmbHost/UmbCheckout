using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using UmbCheckout.Stripe.Notifications;
using UmbCheckout.Stripe.Persistence;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Testing;
using Xunit;
using SettingsModel = UmbCheckout.Stripe.Models.UmbCheckoutStripeSettings;

namespace UmbCheckout.Tests.Persistence;

[Collection(DatabaseCollection.Name)]
public sealed class StripeSettingsServiceTests(SqlServerContainerFixture sqlServer)
{
    public static TheoryData<TestProvider> Providers => new() { TestProvider.Sqlite, TestProvider.SqlServer };

    private async Task<(TestDatabase<UmbCheckoutStripeDbContext> Database, StripeSettingsService Service)> Arrange(TestProvider provider)
    {
        var database = await TestDatabase.CreateAsync<UmbCheckoutStripeDbContext>(provider, sqlServer, o => new UmbCheckoutStripeDbContext(o), UmbCheckoutStripeDbContext.HistoryTable);
        await using (var db = database.CreateContext())
        {
            await db.Database.MigrateAsync();
        }

        var service = new StripeSettingsService(database.ScopeProvider, NullLogger<StripeSettingsService>.Instance,
            TestMapper.Create(new UmbCheckout.Stripe.Composers.UmbCheckoutConfigurationMappingDefinition()), TestLicenseService.Create());
        return (database, service);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task First_save_inserts_then_updates_the_single_row_and_notifies_once_per_save(TestProvider provider)
    {
        var (database, service) = await Arrange(provider);
        await using var _ = database;

        Assert.Null(await service.GetStripeSettingsAsync());
        Assert.True(await service.UpdateStripeSettingsAsync(new SettingsModel { CollectPhoneNumber = true, ShippingAllowedCountries = "GB" }));
        Assert.True(await service.UpdateStripeSettingsAsync(new SettingsModel { CollectPhoneNumber = false, ShippingAllowedCountries = "GB,US", AllowPromotionalCodes = true }));

        var saved = await service.GetStripeSettingsAsync();
        Assert.False(saved!.CollectPhoneNumber);
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
        var (database, service) = await Arrange(provider);
        await using var _ = database;
        await database.ExecuteSqlAsync("INSERT INTO UmbCheckoutStripeSettings (\"Key\", UseLiveApiDetails, ShippingAllowedCountries, CollectPhoneNumber, CollectPromotionalConsent, AllowPromotionalCodes, EnableAbandonedCartRecovery, AllowPromotionalCodesOnRecoveredCarts) VALUES (NULL, 0, NULL, 0, 0, 0, 0, 0)");

        var legacy = await service.GetStripeSettingsAsync();
        Assert.Equal(Guid.Empty, legacy!.Key);

        await service.UpdateStripeSettingsAsync(legacy);
        Assert.NotEqual(Guid.Empty, (await service.GetStripeSettingsAsync())!.Key);
    }
}
