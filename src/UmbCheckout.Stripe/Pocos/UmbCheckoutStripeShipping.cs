using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace UmbCheckout.Stripe.Pocos
{
    [TableName("UmbCheckoutStripeShipping")]
    [PrimaryKey("Id", AutoIncrement = true)]
    [ExplicitColumns]
    public class UmbCheckoutStripeShipping
    {
        [PrimaryKeyColumn(AutoIncrement = true, IdentitySeed = 1)]
        [Column("Id")]
        public long Id { get; set; }

        [Column("Name")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Name { get; set; } = string.Empty;

        [Column("Value")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Value { get; set; } = string.Empty;
    }
}
