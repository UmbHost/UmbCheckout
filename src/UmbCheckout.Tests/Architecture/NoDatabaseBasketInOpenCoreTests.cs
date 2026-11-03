using UmbCheckout.Core.Interfaces;
using Xunit;

namespace UmbCheckout.Tests.Architecture
{
    /// <summary>
    /// Storing baskets in the database is a licensed feature that lives in the paid add-on. Open Core alone has no working database
    /// basket: no database service, mapper or basket table type, and no licence check for one. Only the configuration flags remain.
    /// </summary>
    public class NoDatabaseBasketInOpenCoreTests
    {
        private static readonly System.Reflection.Assembly Core = typeof(IBasketService).Assembly;

        [Fact]
        public void Open_Core_has_no_database_basket_types()
        {
            var offenders = Core.GetTypes()
                .Where(t => t.Name.Contains("DatabaseService", StringComparison.Ordinal)
                            || t.Name.Contains("DatabaseMapper", StringComparison.Ordinal)
                            || t.Name == "UmbCheckoutBasket")
                .Select(t => t.FullName)
                .ToList();

            Assert.Empty(offenders);
        }

        [Fact]
        public void Open_Core_keeps_the_configuration_flags_for_the_add_on()
        {
            var configuration = typeof(UmbCheckout.Shared.Models.UmbCheckoutConfiguration);

            Assert.NotNull(configuration.GetProperty("StoreBasketInDatabase"));
            Assert.NotNull(configuration.GetProperty("BasketInDatabaseExpiry"));
        }
    }
}
