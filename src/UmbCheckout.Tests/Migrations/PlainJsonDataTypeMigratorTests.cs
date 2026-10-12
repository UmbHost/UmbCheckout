using Microsoft.Data.Sqlite;
using NPoco;
using UmbCheckout.Core.Migrations.PlainJson;
using Xunit;

namespace UmbCheckout.Tests.Migrations
{
    public class PlainJsonDataTypeMigratorTests : IDisposable
    {
        private const string UiAlias = "UmbCheckout.PropertyEditorUi.MetaData";
        private const string LegacyAlias = "umbCheckoutMetaData";
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        private readonly Database _db;

        public PlainJsonDataTypeMigratorTests()
        {
            _connection.Open();
            _db = new Database(_connection, DatabaseType.SQLite);
            _db.Execute("CREATE TABLE umbracoDataType (nodeId INTEGER PRIMARY KEY, propertyEditorAlias TEXT, propertyEditorUiAlias TEXT, dbType TEXT, config TEXT)");
            _db.Execute("CREATE TABLE cmsPropertyType (id INTEGER PRIMARY KEY, dataTypeId INTEGER, Alias TEXT)");
            _db.Execute("CREATE TABLE umbracoPropertyData (id INTEGER PRIMARY KEY, versionId INTEGER, propertyTypeId INTEGER, varcharValue TEXT, textValue TEXT)");
        }

        public void Dispose() => _connection.Dispose();

        [Fact]
        public void V13LegacyAlias_MovesVarcharValuesAndUpdatesDataType()
        {
            _db.Execute("INSERT INTO umbracoDataType VALUES (10, @0, NULL, 'Nvarchar', '{}')", LegacyAlias);
            _db.Execute("INSERT INTO cmsPropertyType VALUES (100, 10, 'metaData')");
            _db.Execute("INSERT INTO umbracoPropertyData VALUES (1, 1, 100, '[{\"name\":\"k\",\"value\":\"v\"}]', NULL)");
            _db.Execute("INSERT INTO umbracoPropertyData VALUES (2, 2, 100, '[{\"name\":\"k\",\"value\":\"v2\"}]', NULL)");

            var result = PlainJsonDataTypeMigrator.Migrate(_db, UiAlias, LegacyAlias);

            Assert.Equal(new PlainJsonDataTypeMigrator.MigrationResult(1, 2), result);
            var dataType = _db.Fetch<dynamic>("SELECT * FROM umbracoDataType WHERE nodeId = 10").Single();
            Assert.Equal("Umbraco.Plain.Json", (string)dataType.propertyEditorAlias);
            Assert.Equal(UiAlias, (string)dataType.propertyEditorUiAlias);
            Assert.Equal("Ntext", (string)dataType.dbType);
            Assert.Equal("[{\"name\":\"k\",\"value\":\"v2\"}]", _db.ExecuteScalar<string>("SELECT textValue FROM umbracoPropertyData WHERE id = 2"));
            Assert.Null(_db.ExecuteScalar<string?>("SELECT varcharValue FROM umbracoPropertyData WHERE id = 1"));
        }

        [Fact]
        public void V17BetaPlainString_UpdatesAliasAndKeepsTextValues()
        {
            _db.Execute("INSERT INTO umbracoDataType VALUES (11, 'Umbraco.Plain.String', @0, 'Ntext', '{}')", UiAlias);
            _db.Execute("INSERT INTO cmsPropertyType VALUES (101, 11, 'umbCheckoutMetaData')");
            _db.Execute("INSERT INTO umbracoPropertyData VALUES (3, 1, 101, NULL, '[{\"name\":\"a\",\"value\":\"b\"}]')");

            var result = PlainJsonDataTypeMigrator.Migrate(_db, UiAlias, LegacyAlias);

            Assert.Equal(new PlainJsonDataTypeMigrator.MigrationResult(1, 0), result);
            Assert.Equal("[{\"name\":\"a\",\"value\":\"b\"}]", _db.ExecuteScalar<string>("SELECT textValue FROM umbracoPropertyData WHERE id = 3"));
        }

        [Fact]
        public void SecondRun_IsNoOp()
        {
            _db.Execute("INSERT INTO umbracoDataType VALUES (10, @0, NULL, 'Nvarchar', '{}')", LegacyAlias);
            _db.Execute("INSERT INTO cmsPropertyType VALUES (100, 10, 'metaData')");
            _db.Execute("INSERT INTO umbracoPropertyData VALUES (1, 1, 100, 'x', NULL)");
            PlainJsonDataTypeMigrator.Migrate(_db, UiAlias, LegacyAlias);

            var second = PlainJsonDataTypeMigrator.Migrate(_db, UiAlias, LegacyAlias);

            Assert.Equal(new PlainJsonDataTypeMigrator.MigrationResult(0, 0), second);
            Assert.Equal("x", _db.ExecuteScalar<string>("SELECT textValue FROM umbracoPropertyData WHERE id = 1"));
        }

        [Fact]
        public void UnrelatedDataTypes_AreUntouched()
        {
            _db.Execute("INSERT INTO umbracoDataType VALUES (12, 'Umbraco.Plain.String', 'Umb.PropertyEditorUi.TextBox', 'Nvarchar', '{}')");
            _db.Execute("INSERT INTO cmsPropertyType VALUES (102, 12, 'title')");
            _db.Execute("INSERT INTO umbracoPropertyData VALUES (4, 1, 102, 'Hello', NULL)");

            PlainJsonDataTypeMigrator.Migrate(_db, UiAlias, LegacyAlias);

            Assert.Equal("Hello", _db.ExecuteScalar<string>("SELECT varcharValue FROM umbracoPropertyData WHERE id = 4"));
            Assert.Equal("Umbraco.Plain.String", _db.ExecuteScalar<string>("SELECT propertyEditorAlias FROM umbracoDataType WHERE nodeId = 12"));
        }
    }
}
