using System.Reflection;
using System.Xml.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Models;
using UmbCheckout.Stripe.uSync.Serializers;
using uSync.Core.Serialization;
using Xunit;

namespace UmbCheckout.Tests.uSync
{
    /// <summary>
    /// The uSync Stripe settings file carries every setting (v13 exported only UseLiveApiDetails, so an import reset shipping
    /// countries, phone collection, promotion codes and abandoned-cart recovery), and an older file keeps the site's current values.
    /// </summary>
    public class StripeSettingsSerializerTests
    {
        private readonly Mock<IStripeSettingsService> _service = new();

        private StripeSettingsSerializer Serializer() =>
            new(NullLogger<SyncSerializerRoot<UmbCheckoutStripeSettings>>.Instance, _service.Object);

        private static IEnumerable<PropertyInfo> Settings =>
            typeof(UmbCheckoutStripeSettings).GetProperties().Where(p => p.Name is not nameof(UmbCheckoutStripeSettings.Id) and not nameof(UmbCheckoutStripeSettings.Key));

        private UmbCheckoutStripeSettings Import(XElement node)
        {
            UmbCheckoutStripeSettings? saved = null;
            _service.Setup(s => s.UpdateStripeSettings(It.IsAny<UmbCheckoutStripeSettings>())).Callback<UmbCheckoutStripeSettings>(s => saved = s).ReturnsAsync(true);

            var attempt = Serializer().Deserialize(node, new SyncSerializerOptions());

            Assert.True(attempt.Success, attempt.Message);
            return saved ?? attempt.Item;
        }

        [Fact]
        public void Every_setting_round_trips()
        {
            var settings = new UmbCheckoutStripeSettings();
            foreach (var property in Settings)
            {
                // Reflection, so a setting added later can't be left out of the file.
                property.SetValue(settings, property.PropertyType == typeof(bool) ? true : property.PropertyType == typeof(string) ? "GB,US" : throw new InvalidOperationException($"Unhandled setting type {property.PropertyType} ({property.Name})"));
            }
            _service.Setup(s => s.GetStripeSettings()).ReturnsAsync(new UmbCheckoutStripeSettings());

            var node = Serializer().Serialize(settings, new SyncSerializerOptions()).Item;
            var imported = Import(node);

            foreach (var property in Settings)
            {
                Assert.True(Equals(property.GetValue(settings), property.GetValue(imported)), $"{property.Name} didn't round-trip");
            }
        }

        [Fact]
        public void An_older_file_keeps_the_sites_current_values_for_settings_it_lacks()
        {
            _service.Setup(s => s.GetStripeSettings()).ReturnsAsync(new UmbCheckoutStripeSettings
            {
                ShippingAllowedCountries = "GB",
                CollectPhoneNumber = true,
                CollectPromotionalEmailsConsent = true,
                AllowPromotionalCodes = true,
                EnableAbandonedCartRecovery = true,
                AllowPromotionalCodesOnRecoveredCarts = true
            });
            var key = Guid.NewGuid();
            var node = new XElement("Settings", new XAttribute("Id", 1), new XAttribute("Alias", "UmbCheckoutStripeSettings"), new XAttribute("Key", key),
                new XElement("UseLiveApiDetails", true));

            var imported = Import(node);

            Assert.True(imported.UseLiveApiDetails);
            Assert.Equal("GB", imported.ShippingAllowedCountries);
            Assert.True(imported.CollectPhoneNumber);
            Assert.True(imported.CollectPromotionalEmailsConsent);
            Assert.True(imported.AllowPromotionalCodes);
            Assert.True(imported.EnableAbandonedCartRecovery);
            Assert.True(imported.AllowPromotionalCodesOnRecoveredCarts);
        }

        [Fact]
        public void An_empty_shipping_countries_element_clears_them()
        {
            _service.Setup(s => s.GetStripeSettings()).ReturnsAsync(new UmbCheckoutStripeSettings { ShippingAllowedCountries = "GB", CollectPhoneNumber = true });
            var node = new XElement("Settings", new XAttribute("Id", 1), new XAttribute("Alias", "UmbCheckoutStripeSettings"), new XAttribute("Key", Guid.NewGuid()),
                new XElement("UseLiveApiDetails", false), new XElement("ShippingAllowedCountries", string.Empty), new XElement("CollectPhoneNumber", false));

            var imported = Import(node);

            Assert.Null(imported.ShippingAllowedCountries);
            Assert.False(imported.CollectPhoneNumber);
        }

        [Fact]
        public void A_refused_save_fails_the_import()
        {
            // The save's answer was ignored, so uSync reported success for settings that were never stored.
            _service.Setup(s => s.UpdateStripeSettings(It.IsAny<UmbCheckoutStripeSettings>())).ReturnsAsync(false);

            Assert.Throws<InvalidOperationException>(() => Serializer().SaveItem(new UmbCheckoutStripeSettings()));
        }

        [Fact]
        public void A_failed_save_fails_the_import()
        {
            // The save was fire-and-forget: its failure was never seen.
            _service.Setup(s => s.UpdateStripeSettings(It.IsAny<UmbCheckoutStripeSettings>())).ThrowsAsync(new InvalidOperationException("database down"));

            Assert.Throws<InvalidOperationException>(() => Serializer().SaveItem(new UmbCheckoutStripeSettings()));
        }
    }
}
