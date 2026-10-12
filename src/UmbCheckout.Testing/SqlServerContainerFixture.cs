using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
using Xunit;

namespace UmbCheckout.Testing;

/// <summary>One SQL Server container per test assembly; each test gets its own database. Requires Docker.</summary>
public sealed class SqlServerContainerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public async Task<string> CreateDatabaseAsync()
    {
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
