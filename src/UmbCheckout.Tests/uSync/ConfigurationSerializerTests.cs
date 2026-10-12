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
        var serializer = new ConfigurationSerializer(NullLogger<SyncSerializerRoot<UmbCheckoutConfiguration>>.Instance, Mock.Of<IConfigurationService>());
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

        var node = (await serializer.SerializeAsync(exported, new SyncSerializerOptions())).Item!;
        var imported = (await serializer.DeserializeAsync(node, new SyncSerializerOptions())).Item!;

        Assert.Equal("GBP", imported.CurrencyCode);
        Assert.True(imported.StoreBasketInCookie);
        Assert.Equal(7, imported.BasketInCookieExpiry);
        Assert.True(imported.StoreBasketInDatabase);
        Assert.Equal(45, imported.BasketInDatabaseExpiry);
        Assert.True(imported.EnableShipping);
        Assert.Equal("/basket/", imported.SuccessPageUrl.Single().Url);
        Assert.Equal("/cancel/", imported.CancelPageUrl.Single().Url);
    }
}
