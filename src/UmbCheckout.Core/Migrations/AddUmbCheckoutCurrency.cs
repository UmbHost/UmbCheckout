using Microsoft.Extensions.Logging;
using UmbCheckout.Core.Pocos;
using Umbraco.Cms.Infrastructure.Migrations;

namespace UmbCheckout.Core.Migrations
{
    internal class AddUmbCheckoutCurrencyCode : MigrationBase
    {
        public AddUmbCheckoutCurrencyCode(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
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
        }
    }
}
