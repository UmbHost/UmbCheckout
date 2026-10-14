using System.Reflection;
using System.Xml.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Models;
using UmbCheckout.Stripe.uSync.Serializers;
using uSync.Core.Serialization;
using Xunit;

namespace UmbCheckout.Tests.uSync;

/// <summary>uSync moves the Stripe settings between environments: every setting, and a refused save shows as failed.</summary>
public sealed class StripeSettingsSerializerTests
{
    /// <summary>The real serializer (uSync requires its attribute on the concrete type), with its protected core methods reached by reflection.</summary>
    private sealed class Exposed(IStripeSettingsService service)
    {
        private readonly StripeSettingsSerializer _serializer = new(NullLogger<SyncSerializerRoot<UmbCheckoutStripeSettings>>.Instance, service);

        public async Task<XElement> Serialize(UmbCheckoutStripeSettings item)
            => (await Invoke<XElement>("SerializeCoreAsync", item, new SyncSerializerOptions())).Item.Required();

        public async Task<UmbCheckoutStripeSettings> Deserialize(XElement node)
            => (await Invoke<UmbCheckoutStripeSettings>("DeserializeCoreAsync", node, new SyncSerializerOptions())).Item.Required();

        public Task SaveItemAsync(UmbCheckoutStripeSettings item) => _serializer.SaveItemAsync(item);

        private Task<global::uSync.Core.Models.SyncAttempt<T>> Invoke<T>(string method, params object[] args)
            => (Task<global::uSync.Core.Models.SyncAttempt<T>>)typeof(StripeSettingsSerializer).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Required().Invoke(_serializer, args).Required();
    }

    private static readonly PropertyInfo[] Settings = typeof(UmbCheckoutStripeSettings).GetProperties()
        .Where(p => p.Name is not (nameof(UmbCheckoutStripeSettings.Id) or nameof(UmbCheckoutStripeSettings.Key)))
        .ToArray();

    [Fact]
    public async Task Every_setting_round_trips()
    {
        // Was: only UseLiveApiDetails was exported, and an import reset the rest to defaults.
        var original = new UmbCheckoutStripeSettings { Key = Guid.NewGuid() };
        foreach (var property in Settings)
        {
            property.SetValue(original, property.PropertyType == typeof(bool) ? true : "GB,US");
        }

        var serializer = new Exposed(Mock.Of<IStripeSettingsService>());
        var copy = await serializer.Deserialize(await serializer.Serialize(original));

        Assert.All(Settings, p => Assert.Equal(p.GetValue(original), p.GetValue(copy)));
        Assert.Equal(original.Key, copy.Key);
    }

    [Fact]
    public async Task An_older_file_on_a_site_with_no_settings_keeps_the_defaults()
    {
        var node = new XElement("StripeSettings", new XAttribute("Id", 1), new XAttribute("Alias", "UmbCheckoutStripeSettings"),
            new XAttribute("Key", Guid.NewGuid()), new XElement("UseLiveApiDetails", true));

        var settings = await new Exposed(Mock.Of<IStripeSettingsService>()).Deserialize(node);

        var defaults = new UmbCheckoutStripeSettings();
        Assert.True(settings.UseLiveApiDetails);
        Assert.All(Settings.Where(p => p.Name != nameof(UmbCheckoutStripeSettings.UseLiveApiDetails)), p => Assert.Equal(p.GetValue(defaults), p.GetValue(settings)));
    }

    [Fact]
    public async Task An_older_file_keeps_the_sites_current_values_for_settings_it_does_not_have()
    {
        // Review minor: importing a file from an older version reset every newer setting the site had saved.
        var stored = new UmbCheckoutStripeSettings { Key = Guid.NewGuid() };
        foreach (var property in Settings)
        {
            property.SetValue(stored, property.PropertyType == typeof(bool) ? true : "GB,US");
        }

        var service = new Mock<IStripeSettingsService>();
        service.Setup(s => s.GetStripeSettingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stored);
        var node = new XElement("StripeSettings", new XAttribute("Id", 1), new XAttribute("Alias", "UmbCheckoutStripeSettings"),
            new XAttribute("Key", stored.Key), new XElement("UseLiveApiDetails", false));

        var settings = await new Exposed(service.Object).Deserialize(node);

        Assert.False(settings.UseLiveApiDetails); // in the file: imported
        Assert.All(Settings.Where(p => p.Name != nameof(UmbCheckoutStripeSettings.UseLiveApiDetails)), p => Assert.Equal(p.GetValue(stored), p.GetValue(settings)));
    }

    [Fact]
    public async Task An_empty_country_list_in_the_file_still_clears_it()
    {
        var service = new Mock<IStripeSettingsService>();
        service.Setup(s => s.GetStripeSettingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new UmbCheckoutStripeSettings { ShippingAllowedCountries = "GB" });
        var node = new XElement("StripeSettings", new XAttribute("Id", 1), new XAttribute("Key", Guid.NewGuid()),
            new XElement("UseLiveApiDetails", false), new XElement("ShippingAllowedCountries", string.Empty));

        Assert.Null((await new Exposed(service.Object).Deserialize(node)).ShippingAllowedCountries);
    }

    [Fact]
    public async Task A_refused_import_fails_instead_of_reporting_success()
    {
        var service = new Mock<IStripeSettingsService>();
        service.Setup(s => s.UpdateStripeSettingsAsync(It.IsAny<UmbCheckoutStripeSettings>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidOperationException>(() => new Exposed(service.Object).SaveItemAsync(new UmbCheckoutStripeSettings()));
    }
}
