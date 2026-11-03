using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Models;
using UmbCheckout.uSync.Serializers;
using uSync.Core.Serialization;
using Xunit;

namespace UmbCheckout.Tests.uSync;

public sealed class ConfigurationSerializerTests
{
    [Fact]
    public async Task Every_setting_survives_an_export_and_import()
    {
        var service = new Mock<IConfigurationService>();
        service.Setup(s => s.UpdateConfigurationAsync(It.IsAny<UmbCheckoutConfiguration>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var serializer = new ConfigurationSerializer(NullLogger<SyncSerializerRoot<UmbCheckoutConfiguration>>.Instance, service.Object);
        var exported = new UmbCheckoutConfiguration
        {
            Key = Guid.NewGuid(),
            CurrencyCode = "GBP",
            StoreBasketInCookie = true,
            BasketInCookieExpiry = 7,
            StoreBasketInDatabase = true,
            BasketInDatabaseExpiry = 45,
            EnableShipping = true,
            SuccessPageUrl = [new MultiUrlPicker { Udi = "umb://document/38b0881403074f92930a30751217a7c1", Name = "Basket", Url = "/basket/" }],
            CancelPageUrl = [new MultiUrlPicker { Udi = "umb://document/76df5ca5ee7044e0be5d61b5a8ae5f9f", Name = "Cancel", Url = "/cancel/" }]
        };

        var node = (await serializer.SerializeAsync(exported, new SyncSerializerOptions())).Item.Required();
        var imported = (await serializer.DeserializeAsync(node, new SyncSerializerOptions())).Item.Required();

        Assert.Equal("GBP", imported.CurrencyCode);
        Assert.True(imported.StoreBasketInCookie);
        Assert.Equal(7, imported.BasketInCookieExpiry);
        Assert.True(imported.StoreBasketInDatabase);
        Assert.Equal(45, imported.BasketInDatabaseExpiry);
        Assert.True(imported.EnableShipping);
        Assert.Equal("/basket/", imported.SuccessPageUrl.Single().Url);
        Assert.Equal("/cancel/", imported.CancelPageUrl.Single().Url);
    }

    [Fact]
    public async Task A_refused_import_fails_instead_of_reporting_success()
    {
        // e.g. a site's Saving handler cancelled it: uSync must show the item as failed.
        var service = new Mock<IConfigurationService>();
        service.Setup(s => s.UpdateConfigurationAsync(It.IsAny<UmbCheckoutConfiguration>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var serializer = new ConfigurationSerializer(NullLogger<SyncSerializerRoot<UmbCheckoutConfiguration>>.Instance, service.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => serializer.SaveItemAsync(new UmbCheckoutConfiguration()));
    }
}