using Microsoft.Extensions.Logging;
using UmbCheckout.Core.Migrations.PlainJson;
using Umbraco.Cms.Infrastructure.Migrations;

namespace UmbCheckout.Core.Migrations
{
    internal class MigrateMetaDataToPlainJson : AsyncMigrationBase
    {
        public MigrateMetaDataToPlainJson(IMigrationContext context) : base(context) { }

        protected override Task MigrateAsync()
        {
            var result = PlainJsonDataTypeMigrator.Migrate(Database, "UmbCheckout.PropertyEditorUi.MetaData", "umbCheckoutMetaData");
            Logger.LogInformation("UmbCheckout MetaData: {DataTypes} data types moved to Umbraco.Plain.Json, {Values} values moved", result.DataTypesUpdated, result.ValuesMoved);
            return Task.CompletedTask;
        }
    }
}
