using Microsoft.EntityFrameworkCore;

namespace UmbCheckout.Shared.Persistence;

/// <summary>Options for <c>dotnet ef</c>: set UMBCHECKOUT_EF_PROVIDER to SqlServer or Sqlite. Nothing connects at design time.</summary>
public static class DesignTimeOptions
{
    public const string EnvironmentVariable = "UMBCHECKOUT_EF_PROVIDER";

    public static DbContextOptions<TContext> For<TContext>(string historyTable) where TContext : DbContext
    {
        var builder = new DbContextOptionsBuilder<TContext>();
        switch (Environment.GetEnvironmentVariable(EnvironmentVariable))
        {
            case "SqlServer":
                builder.UseUmbCheckoutProvider("Microsoft.Data.SqlClient", "Server=.;Database=UmbCheckoutDesignTime;Trusted_Connection=True;TrustServerCertificate=True", historyTable);
                break;
            case "Sqlite":
                builder.UseUmbCheckoutProvider("Microsoft.Data.Sqlite", "Data Source=umbcheckout-design-time.db", historyTable);
                break;
            default:
                throw new InvalidOperationException($"Set {EnvironmentVariable} to SqlServer or Sqlite before running dotnet ef.");
        }

        return builder.Options;
    }
}
