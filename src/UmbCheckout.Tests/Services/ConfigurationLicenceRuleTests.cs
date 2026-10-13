using System.Data;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NPoco.Linq;
using UmbCheckout.Backoffice.Controllers.Api;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Services;
using UmbCheckout.Shared.Models;
using UmbHost.Licensing.Models;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Xunit;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;
using ConfigurationPoco = UmbCheckout.Core.Pocos.UmbCheckoutConfiguration;

namespace UmbCheckout.Tests.Services;

/// <summary>
/// The basket storage licence rule belongs to the licensed add-on, not the open package: with
/// only the open package, the configuration is stored and read as given. The open tests run unlicensed (they can't switch the licence).
/// </summary>
public sealed class ConfigurationLicenceRuleTests
{
    [Fact]
    public void The_open_tests_run_unlicensed()
    {
        Assert.False(UmbCheckoutSettings.IsLicensed);
    }

    [Fact]
    public async Task The_open_service_reads_basket_storage_as_stored()
    {
        var stored = new ConfigurationPoco { Id = 1, Key = Guid.NewGuid(), StoreBasketInCookie = true, StoreBasketInDatabase = true };
        var (service, mapper, _) = Arrange(stored);
        ConfigurationPoco? mapped = null;
        mapper.Setup(m => m.Map<ConfigurationPoco, UmbCheckoutConfiguration>(It.IsAny<ConfigurationPoco>()))
            .Callback<object?>(p => mapped = (ConfigurationPoco?)p)
            .Returns<object?>(p => new UmbCheckoutConfiguration
            {
                StoreBasketInCookie = ((ConfigurationPoco)p!).StoreBasketInCookie,
                StoreBasketInDatabase = ((ConfigurationPoco)p!).StoreBasketInDatabase
            });

        var configuration = await service.GetConfiguration();

        Assert.True(mapped!.StoreBasketInCookie);
        Assert.True(configuration!.StoreBasketInCookie);
        Assert.True(configuration.StoreBasketInDatabase);
    }

    [Fact]
    public async Task The_open_service_saves_basket_storage_as_given()
    {
        var (service, mapper, database) = Arrange(stored: null);
        UmbCheckoutConfiguration? given = null;
        mapper.Setup(m => m.Map<UmbCheckoutConfiguration, ConfigurationPoco>(It.IsAny<UmbCheckoutConfiguration>()))
            .Callback<object?>(c => given = (UmbCheckoutConfiguration?)c)
            .Returns<object?>(c => new ConfigurationPoco
            {
                StoreBasketInCookie = ((UmbCheckoutConfiguration)c!).StoreBasketInCookie,
                StoreBasketInDatabase = ((UmbCheckoutConfiguration)c!).StoreBasketInDatabase
            });
        ConfigurationPoco? inserted = null;
        database.Setup(d => d.InsertAsync(It.IsAny<ConfigurationPoco>()))
            .Callback<ConfigurationPoco>(p => inserted = p)
            .ReturnsAsync(1);

        var saved = await service.UpdateConfiguration(new UmbCheckoutConfiguration { StoreBasketInCookie = true, StoreBasketInDatabase = true, CurrencyCode = "GBP" });

        Assert.True(saved);
        Assert.True(given!.StoreBasketInCookie);
        Assert.True(given.StoreBasketInDatabase);
        Assert.True(inserted!.StoreBasketInCookie);
        Assert.True(inserted.StoreBasketInDatabase);
    }

    [Theory]
    [InlineData("true", "true", true, true)]
    [InlineData("true", "false", true, false)]
    // Database storage without the cookie is passed on as given: turning the cookie on is the add-on's rule
    [InlineData("false", "true", false, true)]
    public async Task The_backoffice_api_passes_basket_storage_on_as_given(string cookie, string database, bool expectedCookie, bool expectedDatabase)
    {
        var configurationService = new Mock<IConfigurationService>();
        UmbCheckoutConfiguration? given = null;
        configurationService.Setup(s => s.UpdateConfiguration(It.IsAny<UmbCheckoutConfiguration>()))
            .Callback<UmbCheckoutConfiguration>(c => given = c)
            .ReturnsAsync(true);
        var controller = new ConfigurationApiController(configurationService.Object, NullLogger<ConfigurationApiController>.Instance, null!,
            Mock.Of<ILocalizedTextService>(), Options.Create(new WebRoutingSettings()));

        await controller.UpdateConfiguration(new ConfigurationValue
        {
            StoreBasketInCookie = cookie,
            StoreBasketInDatabase = database,
            EnableShipping = "false",
            CurrencyCode = "gbp"
        });

        Assert.Equal(expectedCookie, given!.StoreBasketInCookie);
        Assert.Equal(expectedDatabase, given.StoreBasketInDatabase);
        Assert.Equal("GBP", given.CurrencyCode);
    }

    private static (ConfigurationService Service, Mock<IUmbracoMapper> Mapper, Mock<IUmbracoDatabase> Database) Arrange(ConfigurationPoco? stored)
    {
        var query = new Mock<IAsyncQueryProviderWithIncludes<ConfigurationPoco>>();
        query.Setup(q => q.SingleOrDefault()).ReturnsAsync(stored!);
        var database = new Mock<IUmbracoDatabase>();
        database.Setup(d => d.QueryAsync<ConfigurationPoco>()).Returns(query.Object);

        var scope = new Mock<IScope>();
        scope.SetupGet(s => s.Database).Returns(database.Object);
        scope.SetupGet(s => s.Notifications).Returns(Mock.Of<IScopedNotificationPublisher>());
        var scopeProvider = new Mock<IScopeProvider>();
        scopeProvider.Setup(p => p.CreateScope(It.IsAny<IsolationLevel>(), It.IsAny<RepositoryCacheMode>(), It.IsAny<IEventDispatcher?>(),
                It.IsAny<IScopedNotificationPublisher?>(), It.IsAny<bool?>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(scope.Object);
        var coreScopeProvider = new Mock<ICoreScopeProvider>();
        coreScopeProvider.Setup(p => p.CreateCoreScope(It.IsAny<IsolationLevel>(), It.IsAny<RepositoryCacheMode>(), It.IsAny<IEventDispatcher?>(),
                It.IsAny<IScopedNotificationPublisher?>(), It.IsAny<bool?>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(scope.Object);

        var mapper = new Mock<IUmbracoMapper>();
        var service = new ConfigurationService(scopeProvider.Object, NullLogger<ConfigurationService>.Instance, mapper.Object, coreScopeProvider.Object, null!, new UmbCheckout.Shared.Persistence.SettingsCache<ConfigurationPoco>());
        return (service, mapper, database);
    }
}
