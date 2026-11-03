using Microsoft.EntityFrameworkCore.Design;
using UmbCheckout.Shared.Persistence;

namespace UmbCheckout.Tests.Persistence.Probe;

public sealed class ProbeDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ProbeDbContext>
{
    public ProbeDbContext CreateDbContext(string[] args) => new(DesignTimeOptions.For<ProbeDbContext>(ProbeDbContext.HistoryTable));
}
