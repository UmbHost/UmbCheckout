using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace UmbCheckout.Stripe.Migrations
{
    internal class AddUmbCheckoutStripeShippingTable : MigrationBase
    {
        public AddUmbCheckoutStripeShippingTable(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            Logger.LogDebug("Running migration {MigrationStep}", "UmbCheckoutStripeShippingTable");

            if (TableExists("UmbCheckoutStripeShipping") == false)
            {
                Create.Table<UmbCheckoutStripeShippingSchema>().Do();
            }
            else
            {
                Logger.LogDebug("The database table {DbTable} already exists, skipping", "UmbCheckoutStripeShipping");
            }
        }

        [TableName("UmbCheckoutStripeShipping")]
        [PrimaryKey("Id", AutoIncrement = true)]
        [ExplicitColumns]
        public class UmbCheckoutStripeShippingSchema
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
}
