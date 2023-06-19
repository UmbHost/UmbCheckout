using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace UmbCheckout.Core.Pocos
{
    [TableName("UmbCheckoutConfiguration")]
    [PrimaryKey("Id", AutoIncrement = true)]
    [ExplicitColumns]
    internal class UmbCheckoutConfiguration
    {
        [PrimaryKeyColumn(AutoIncrement = true, IdentitySeed = 1)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("SuccessPageUrl")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string SuccessPageUrl { get; set; } = string.Empty;

        [Column("CancelPageUrl")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string CancelPageUrl { get; set; } = string.Empty;

        [Column("StoreBasketInCookie")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public bool StoreBasketInCookie { get; set; }

        [Column("StoreBasketInDatabase")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public bool StoreBasketInDatabase { get; set; }

        [Column("BasketInDatabaseExpiry")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int BasketInDatabaseExpiry { get; set; } = 30;

        [Column("BasketInCookieExpiry")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int BasketInCookieExpiry { get; set; } = 30;

        [Column("EnableShipping")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public bool EnableShipping { get; set; }
    }
}
