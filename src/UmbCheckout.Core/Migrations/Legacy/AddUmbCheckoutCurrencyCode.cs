using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace UmbCheckout.Core.Migrations.Legacy
{
    /// <summary>Frozen pre-v17 step: only runs on installs that started the NPoco plan. Never change it.</summary>
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
                AddColumn<CurrencyCodeColumn>("UmbCheckoutConfiguration", "CurrencyCode");
            }
            else
            {
                Logger.LogDebug("The database table {DbTable} already exists, skipping", "UmbCheckoutAddCurrencyCode");
            }

            return Task.CompletedTask;
        }

        [TableName("UmbCheckoutConfiguration")]
        [ExplicitColumns]
        private sealed class CurrencyCodeColumn
        {
            [Column("CurrencyCode")]
            [NullSetting(NullSetting = NullSettings.Null)]
            public string CurrencyCode { get; set; } = string.Empty;
        }
    }
}
