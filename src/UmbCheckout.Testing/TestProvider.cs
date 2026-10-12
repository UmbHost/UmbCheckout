namespace UmbCheckout.Testing;

/// <summary>The database a persistence test runs on.</summary>
public enum TestProvider
{
    Sqlite,
    SqlServer,

    /// <summary>A SQLite file opened like Umbraco does (Cache=Shared, pooled): several connections share one cache and its locks.</summary>
    SqliteSharedCacheFile,
}
