using Microsoft.EntityFrameworkCore;
using UmbCheckout.Stripe.Models;
using UmbCheckout.Stripe.Notifications;
using UmbCheckout.Stripe.Persistence;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Testing;
using Xunit;

namespace UmbCheckout.Tests.Persistence;

[Collection(DatabaseCollection.Name)]
public sealed class ShippingRateDatabaseServiceTests(SqlServerContainerFixture sqlServer)
{
    public static TheoryData<TestProvider> Providers => new() { TestProvider.Sqlite, TestProvider.SqlServer };

    private async Task<(TestDatabase<UmbCheckoutStripeDbContext> Database, StripeShippingRateDatabaseService Service)> Arrange(TestProvider provider)
    {
        var database = await TestDatabase.CreateAsync<UmbCheckoutStripeDbContext>(provider, sqlServer, o => new UmbCheckoutStripeDbContext(o), UmbCheckoutStripeDbContext.HistoryTable);
        await using (var db = database.CreateContext())
        {
            await db.Database.MigrateAsync();
        }

        var service = new StripeShippingRateDatabaseService(database.ScopeProvider, TestMapper.Create(new UmbCheckout.Stripe.Composers.UmbCheckoutConfigurationMappingDefinition()));
        return (database, service);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Create_get_update_delete_round_trip_with_notifications(TestProvider provider)
    {
        var (database, service) = await Arrange(provider);
        await using var _ = database;

        var created = await service.CreateShippingRateAsync(new ShippingRate { Name = "Standard", Value = "shr_1" });
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Key);
        Assert.Null(await service.CreateShippingRateAsync(new ShippingRate { Name = "Dup", Value = "shr_1" }));

        var updated = await service.UpdateShippingRateAsync(new ShippingRate { Name = "Standard UK", Value = "shr_1" });
        Assert.Equal("Standard UK", updated!.Name);
        Assert.Equal(created.Key, updated.Key);
        Assert.Equal("Standard UK", (await service.GetShippingRateAsync(created.Key))!.Name);
        Assert.Equal("Standard UK", (await service.GetShippingRateAsync("shr_1"))!.Name);

        Assert.True(await service.DeleteShippingRateAsync(created.Key));
        Assert.False(await service.DeleteShippingRateAsync(created.Key));
        Assert.Empty(await service.GetShippingRatesAsync());
        Assert.Equal(2, database.ScopeProvider.Notifications.Published.OfType<OnShippingRateSavedNotification>().Count());
        Assert.Single(database.ScopeProvider.Notifications.Published.OfType<OnShippingRateDeletedNotification>());
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Update_of_an_unknown_rate_creates_it(TestProvider provider)
    {
        var (database, service) = await Arrange(provider);
        await using var _ = database;

        var result = await service.UpdateShippingRateAsync(new ShippingRate { Name = "New", Value = "shr_new" });

        Assert.Equal("shr_new", result!.Value);
        Assert.Single(await service.GetShippingRatesAsync());
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task A_legacy_rate_with_a_null_key_gets_a_key_when_updated(TestProvider provider)
    {
        var (database, service) = await Arrange(provider);
        await using var _ = database;
        await database.ExecuteSqlAsync("INSERT INTO UmbCheckoutStripeShipping (Name, Value, \"Key\") VALUES ('Old', 'shr_old', NULL)");

        Assert.Equal(Guid.Empty, (await service.GetShippingRateAsync("shr_old"))!.Key);
        var updated = await service.UpdateShippingRateAsync(new ShippingRate { Name = "Old", Value = "shr_old" });

        Assert.NotEqual(Guid.Empty, updated!.Key);
    }
}
