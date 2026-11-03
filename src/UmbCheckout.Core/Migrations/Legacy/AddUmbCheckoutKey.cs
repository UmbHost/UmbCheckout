using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace UmbCheckout.Core.Migrations.Legacy
{
    /// <summary>Frozen pre-v17 step: only runs on installs that started the NPoco plan. Never change it.</summary>
    internal class AddUmbCheckoutKey : AsyncMigrationBase
    {
        public AddUmbCheckoutKey(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            Logger.LogDebug("Running migration {MigrationStep}", "UmbCheckoutAddConfigurationKey");

            if (!ColumnExists("UmbCheckoutConfiguration", "Key"))
            {
                AddColumn<KeyColumn>("UmbCheckoutConfiguration", "Key");
            }
            else
            {
                Logger.LogDebug("The database table {DbTable} already exists, skipping", "UmbCheckoutConfigurationKey");
            }

            return Task.CompletedTask;
        }

        [TableName("UmbCheckoutConfiguration")]
        [ExplicitColumns]
        private sealed class KeyColumn
        {
            [Column("Key")]
            [NullSetting(NullSetting = NullSettings.Null)]
            public Guid Key { get; set; }
        }
    }
}
