using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Models;
using UmbCheckout.uSync.Serializers;
using uSync.Core.Serialization;
using Xunit;

namespace UmbCheckout.Tests.uSync
{
    /// <summary>A configuration import whose save is refused or fails shows as failed in uSync, not as a success.</summary>
    public class ConfigurationSerializerTests
    {
        private readonly Mock<IConfigurationService> _service = new();

        private ConfigurationSerializer Serializer() =>
            new(NullLogger<SyncSerializerRoot<UmbCheckoutConfiguration>>.Instance, _service.Object);

        [Fact]
        public void A_refused_save_fails_the_import()
        {
            _service.Setup(s => s.UpdateConfiguration(It.IsAny<UmbCheckoutConfiguration>())).ReturnsAsync(false);

            Assert.Throws<InvalidOperationException>(() => Serializer().SaveItem(new UmbCheckoutConfiguration()));
        }

        [Fact]
        public void A_failed_save_fails_the_import()
        {
            // The save was fire-and-forget: its failure was never seen.
            _service.Setup(s => s.UpdateConfiguration(It.IsAny<UmbCheckoutConfiguration>())).ThrowsAsync(new InvalidOperationException("database down"));

            Assert.Throws<InvalidOperationException>(() => Serializer().SaveItem(new UmbCheckoutConfiguration()));
        }

        [Fact]
        public void The_cookie_expiry_and_the_database_expiry_are_imported_each_from_its_own_element()
        {
            // The import set the cookie expiry from the database expiry's element.
            UmbCheckoutConfiguration? saved = null;
            _service.Setup(s => s.UpdateConfiguration(It.IsAny<UmbCheckoutConfiguration>())).Callback<UmbCheckoutConfiguration>(c => saved = c).ReturnsAsync(true);
            var exported = new UmbCheckoutConfiguration { BasketInCookieExpiry = 7, BasketInDatabaseExpiry = 45 };

            var node = Serializer().Serialize(exported, new SyncSerializerOptions()).Item;
            var attempt = Serializer().Deserialize(node, new SyncSerializerOptions());

            Assert.True(attempt.Success, attempt.Message);
            Assert.Equal(7, (saved ?? attempt.Item).BasketInCookieExpiry);
            Assert.Equal(45, (saved ?? attempt.Item).BasketInDatabaseExpiry);
        }
    }
}
