using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UmbCheckout.Core.Pocos;

namespace UmbCheckout.Core.Persistence;

/// <summary>Maps the configuration row onto the table NPoco created before v17, column for column (see the legacy fixtures).</summary>
internal sealed class UmbCheckoutConfigurationConfiguration(bool isSqlite) : IEntityTypeConfiguration<UmbCheckoutConfiguration>
{
    public void Configure(EntityTypeBuilder<UmbCheckoutConfiguration> entity)
    {
        entity.ToTable("UmbCheckoutConfiguration");
        entity.HasKey(x => x.Id).HasName("PK_UmbCheckoutConfiguration");
        entity.Property(x => x.Id).ValueGeneratedOnAdd();
        entity.Property(x => x.Key).IsRequired(false);
        entity.Property(x => x.CurrencyCode).HasMaxLength(255).IsRequired(false);
        entity.Property(x => x.SuccessPageUrl).HasMaxLength(255).IsRequired();
        entity.Property(x => x.CancelPageUrl).HasMaxLength(255).IsRequired();

        if (isSqlite)
        {
            entity.Property(x => x.CurrencyCode).UseCollation("NOCASE");
            entity.Property(x => x.SuccessPageUrl).UseCollation("NOCASE");
            entity.Property(x => x.CancelPageUrl).UseCollation("NOCASE");
        }
    }
}
