using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace UmbCheckout.Core.Pocos
{

    [TableName("UmbCheckoutBaskets")]
    [PrimaryKey("Id", AutoIncrement = true)]
    [ExplicitColumns]
    public class UmbCheckoutBasket
    {
        [PrimaryKeyColumn(AutoIncrement = true, IdentitySeed = 1)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("SessionId")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string SessionId { get; set; } = string.Empty;

        [Column("Total")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public decimal BasketTotal { get; set; }

        [Column("LineItems")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string LineItems { get; set; } = string.Empty;

        [Column("CreatedDateTime")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public DateTime CreatedDateTime { get; private set; } = DateTime.Now;

        [Column("LastUpdatedDateTime")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public DateTime? LastUpdatedDateTime { get; set; } = DateTime.Now;
    }
}
