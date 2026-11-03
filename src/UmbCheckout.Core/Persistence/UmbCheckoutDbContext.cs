using Microsoft.EntityFrameworkCore;
using UmbCheckout.Core.Pocos;

namespace UmbCheckout.Core.Persistence;

/// <summary>UmbCheckout's configuration table, on Umbraco's database.</summary>
public sealed class UmbCheckoutDbContext(DbContextOptions<UmbCheckoutDbContext> options) : DbContext(options)
{
    public const string HistoryTable = "__UmbCheckoutMigrations";

    internal DbSet<UmbCheckoutConfiguration> Configurations => Set<UmbCheckoutConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfiguration(new UmbCheckoutConfigurationConfiguration(Database.IsSqlite()));
}
