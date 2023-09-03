using Microsoft.Extensions.Logging;
using UmbCheckout.Core.Pocos;
using Umbraco.Cms.Infrastructure.Migrations;

namespace UmbCheckout.Core.Migrations
{
    internal class AddUmbCheckoutKey : MigrationBase
    {
        public AddUmbCheckoutKey(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            Logger.LogDebug("Running migration {MigrationStep}", "UmbCheckoutAddConfigurationKey");

            if (!ColumnExists("UmbCheckoutConfiguration", "Key"))
            {
                AddColumn<UmbCheckoutConfiguration>("UmbCheckoutConfiguration", "Key");
            }
            else
            {
                Logger.LogDebug("The database table {DbTable} already exists, skipping", "UmbCheckoutConfigurationKey");
            }
        }
    }
}
