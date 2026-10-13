using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
using Xunit;

namespace UmbCheckout.Testing;

/// <summary>
/// One SQL Server container per test assembly; each test gets its own database. Requires Docker.
/// The container starts on the first <see cref="CreateDatabaseAsync"/>, so tests that share the collection without using SQL
/// Server (e.g. for the static licence) run on a machine without Docker. Tests that do use it are
/// <c>[Trait("Category", "Integration")]</c>.
/// </summary>
public sealed class SqlServerContainerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    private readonly Lazy<Task> _started;

    public SqlServerContainerFixture() => _started = new Lazy<Task>(() => _container.StartAsync());

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public async Task<string> CreateDatabaseAsync()
    {
        await _started.Value;
        var name = "t" + Guid.NewGuid().ToString("N");
        var master = _container.GetConnectionString();
        await using (var connection = new SqlConnection(master))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE [{name}]";
            await command.ExecuteNonQueryAsync();
        }

        return new SqlConnectionStringBuilder(master) { InitialCatalog = name }.ConnectionString;
    }
}
