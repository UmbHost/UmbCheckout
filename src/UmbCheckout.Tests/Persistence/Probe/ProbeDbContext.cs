using Microsoft.EntityFrameworkCore;

namespace UmbCheckout.Tests.Persistence.Probe;

public sealed class ProbeRow
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}

/// <summary>A tiny context that proves the per-provider migrations, history table and adoption work on both databases.</summary>
public sealed class ProbeDbContext(DbContextOptions<ProbeDbContext> options) : DbContext(options)
{
    public const string HistoryTable = "__ProbeMigrations";

    public DbSet<ProbeRow> Rows => Set<ProbeRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var row = modelBuilder.Entity<ProbeRow>();
        row.ToTable("ProbeRows");
        row.Property(x => x.Name).HasMaxLength(255);
        if (Database.IsSqlite())
        {
            row.Property(x => x.Amount).HasColumnType("TEXT");
        }
        else
        {
            row.Property(x => x.Amount).HasPrecision(20, 9);
        }
    }
}
