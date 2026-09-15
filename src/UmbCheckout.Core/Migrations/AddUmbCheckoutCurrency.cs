using Microsoft.Extensions.Logging;
using UmbCheckout.Core.Pocos;
using Umbraco.Cms.Infrastructure.Migrations;

namespace UmbCheckout.Core.Migrations
{
    internal class AddUmbCheckoutCurrencyCode : AsyncMigrationBase
    {
        public AddUmbCheckoutCurrencyCode(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            Logger.LogDebug("Running migration {MigrationStep}", "UmbCheckoutAddCurrencyCode");

            if (!ColumnExists("UmbCheckoutConfiguration", "CurrencyCode"))
            {
                AddColumn<UmbCheckoutConfiguration>("UmbCheckoutConfiguration", "CurrencyCode");
            }
            else
            {
                Logger.LogDebug("The database table {DbTable} already exists, skipping", "UmbCheckoutAddCurrencyCode");
            }
            return Task.CompletedTask;
        }
    }
}
