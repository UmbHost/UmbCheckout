using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UmbCheckout.Shared.Persistence;

namespace UmbCheckout.Testing;

public static class TestDatabase
{
    /// <summary>A fresh, empty database for one test: SQLite in memory, or a new database on the shared SQL Server container.</summary>
    public static async Task<TestDatabase<TContext>> CreateAsync<TContext>(
        TestProvider provider,
        SqlServerContainerFixture sqlServer,
        Func<DbContextOptions<TContext>, TContext> factory,
        string historyTable)
        where TContext : DbContext
    {
        DbConnection connection = provider == TestProvider.Sqlite
            ? new SqliteConnection("Data Source=:memory:")
            : new Microsoft.Data.SqlClient.SqlConnection(await sqlServer.CreateDatabaseAsync());
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<TContext>();
        options.UseUmbCheckoutProvider(connection, historyTable);
        return new TestDatabase<TContext>(provider, connection, options.Options, factory);
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

    internal TestDatabase(TestProvider provider, DbConnection connection, DbContextOptions<TContext> options, Func<DbContextOptions<TContext>, TContext> factory)
    {
        Provider = provider;
        _connection = connection;
        Options = options;
        _factory = factory;
        ScopeProvider = new TestEfCoreScopeProvider<TContext>(CreateContext);
    }

    public TestProvider Provider { get; }

    public DbContextOptions<TContext> Options { get; }

    public TestEfCoreScopeProvider<TContext> ScopeProvider { get; }

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

    public async ValueTask DisposeAsync() => await _connection.DisposeAsync();
}
