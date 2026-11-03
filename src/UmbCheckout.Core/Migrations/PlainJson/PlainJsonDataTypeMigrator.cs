using NPoco;

namespace UmbCheckout.Core.Migrations.PlainJson
{
    /// <summary>
    /// Moves an UmbCheckout property editor's data types to Umbraco.Plain.Json and moves stored values from varcharValue to textValue.
    /// Handles both v13 data types (legacy schema alias) and v17-beta data types (Umbraco.Plain.String with the UmbCheckout UI alias). Idempotent.
    /// </summary>
    public static class PlainJsonDataTypeMigrator
    {
        public const string PlainJsonAlias = "Umbraco.Plain.Json";
        private const string PlainStringAlias = "Umbraco.Plain.String";

        public sealed record MigrationResult(int DataTypesUpdated, int ValuesMoved);

        public static MigrationResult Migrate(IDatabase database, string editorUiAlias, string legacyEditorAlias)
        {
            var dataTypeIds = database.Fetch<int>(
                "SELECT nodeId FROM umbracoDataType WHERE propertyEditorAlias = @0 OR (propertyEditorAlias = @1 AND propertyEditorUiAlias = @2) OR (propertyEditorAlias = @3 AND propertyEditorUiAlias = @2 AND dbType <> 'Ntext')",
                legacyEditorAlias, PlainStringAlias, editorUiAlias, PlainJsonAlias);

            var allIds = database.Fetch<int>(
                "SELECT nodeId FROM umbracoDataType WHERE propertyEditorUiAlias = @0 OR propertyEditorAlias = @1",
                editorUiAlias, legacyEditorAlias);

            var updated = dataTypeIds.Count == 0
                ? 0
                : database.Execute(
                    "UPDATE umbracoDataType SET propertyEditorAlias = @0, propertyEditorUiAlias = @1, dbType = 'Ntext' WHERE nodeId IN (@2)",
                    PlainJsonAlias, editorUiAlias, dataTypeIds);

            var moved = allIds.Count == 0
                ? 0
                : database.Execute(
                    "UPDATE umbracoPropertyData SET textValue = varcharValue, varcharValue = NULL WHERE varcharValue IS NOT NULL AND propertyTypeId IN (SELECT id FROM cmsPropertyType WHERE dataTypeId IN (@0))",
                    allIds);

            return new MigrationResult(updated, moved);
        }
    }
}
