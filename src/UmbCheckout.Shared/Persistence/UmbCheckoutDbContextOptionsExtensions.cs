using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UmbCheckout.Shared.Persistence;

public static class UmbCheckoutDbContextOptionsExtensions
{
    /// <summary>
    /// Configures a context for Umbraco's database (the provider and connection string Umbraco passes to
    /// <c>AddUmbracoDbContext</c>), with its own migrations history table and per-provider migrations.
    /// </summary>
    public static void UseUmbCheckoutProvider(this DbContextOptionsBuilder options, string? providerName, string? connectionString, string historyTable)
    {
        switch (providerName)
        {
            case "Microsoft.Data.SqlClient":
                options.UseSqlServer(connectionString, x => x.MigrationsHistoryTable(historyTable));
                break;
            case "Microsoft.Data.Sqlite":
            case "Microsoft.Data.SQLite":
                options.UseSqlite(connectionString, x => x.MigrationsHistoryTable(historyTable));
                break;
            default:
                throw new NotSupportedException($"UmbCheckout supports SQL Server and SQLite, not '{providerName}'.");
        }

        Finish(options);
    }

    /// <summary>The same, on an open connection (tests).</summary>
    public static void UseUmbCheckoutProvider(this DbContextOptionsBuilder options, DbConnection connection, string historyTable)
    {
        if (connection is SqliteConnection)
        {
            options.UseSqlite(connection, x => x.MigrationsHistoryTable(historyTable));
        }
        else
        {
            options.UseSqlServer(connection, x => x.MigrationsHistoryTable(historyTable));
        }

        Finish(options);
    }

    private static void Finish(DbContextOptionsBuilder options)
    {
        options.ReplaceService<IMigrationsAssembly, ProviderMigrationsAssembly>();
    }
}
