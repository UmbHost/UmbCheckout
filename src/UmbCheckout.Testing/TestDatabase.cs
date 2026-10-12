using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UmbCheckout.Shared.Persistence;

namespace UmbCheckout.Testing;

public static class TestDatabase
{
    /// <summary>A fresh, empty database for one test: SQLite in memory or in a shared-cache file, or a new database on the shared SQL Server container.</summary>
    public static async Task<TestDatabase<TContext>> CreateAsync<TContext>(
        TestProvider provider,
        SqlServerContainerFixture sqlServer,
        Func<DbContextOptions<TContext>, TContext> factory,
        string historyTable)
        where TContext : DbContext
    {
        var options = new DbContextOptionsBuilder<TContext>();
        DbConnection connection;
        string? file = null;
        switch (provider)
        {
            case TestProvider.Sqlite:
                connection = new SqliteConnection("Data Source=:memory:");
                options.UseUmbCheckoutProvider(connection, historyTable);
                break;
            case TestProvider.SqliteSharedCacheFile:
                file = Path.Combine(Path.GetTempPath(), "umbcheckout-test-" + Guid.NewGuid().ToString("N") + ".db");
                var connectionString = $"Data Source={file};Cache=Shared;Foreign Keys=True;Pooling=True";
                connection = new SqliteConnection(connectionString);
                options.UseUmbCheckoutProvider("Microsoft.Data.Sqlite", connectionString, historyTable);
                break;
            default:
                connection = new Microsoft.Data.SqlClient.SqlConnection(await sqlServer.CreateDatabaseAsync());
                options.UseUmbCheckoutProvider(connection, historyTable);
                break;
        }

        await connection.OpenAsync();
        return new TestDatabase<TContext>(provider, connection, options.Options, factory, file);
    }
}

/// <summary>
/// One test database. SQL passed to <see cref="ExecuteSqlAsync"/> is run raw: don't put "{" or "}" in it
/// (EF treats them as format placeholders); insert JSON through the context instead.
/// </summary>
public sealed class TestDatabase<TContext> : IAsyncDisposable where TContext : DbContext
{
    private readonly DbConnection _connection;
    private readonly Func<DbContextOptions<TContext>, TContext> _factory;
    private readonly string? _file;

    internal TestDatabase(TestProvider provider, DbConnection connection, DbContextOptions<TContext> options, Func<DbContextOptions<TContext>, TContext> factory, string? file)
    {
        Provider = provider;
        _connection = connection;
        Options = options;
        _factory = factory;
        _file = file;
        ScopeProvider = new TestEfCoreScopeProvider<TContext>(CreateContext);
        ContextFactory = new Factory(this);
    }

    public TestProvider Provider { get; }

    public DbContextOptions<TContext> Options { get; }

    public TestEfCoreScopeProvider<TContext> ScopeProvider { get; }

    /// <summary>What Umbraco registers as <see cref="IDbContextFactory{TContext}"/>: a new context with no ambient transaction.</summary>
    public IDbContextFactory<TContext> ContextFactory { get; }

    public TContext CreateContext() => _factory(Options);

    public async Task ExecuteSqlAsync(string sql)
    {
        await using var db = CreateContext();
        await db.Database.ExecuteSqlRawAsync(sql);
    }

    public Task ExecuteSqlFileAsync(string path) => ExecuteSqlAsync(File.ReadAllText(path));

    public async Task<T?> ScalarAsync<T>(string sql)
    {
        await using var command = _connection.CreateCommand();
        command.CommandText = sql;
        var value = await command.ExecuteScalarAsync();
        return value is null or DBNull ? default : (T)Convert.ChangeType(value, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));
    }

    public async Task<IReadOnlyList<string>> AppliedMigrationsAsync()
    {
        await using var db = CreateContext();
        return (await db.Database.GetAppliedMigrationsAsync()).ToList();
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
        if (_file is not null)
        {
            SqliteConnection.ClearAllPools();
            File.Delete(_file);
        }
    }

    private sealed class Factory(TestDatabase<TContext> database) : IDbContextFactory<TContext>
    {
        public TContext CreateDbContext() => database.CreateContext();
    }
}
