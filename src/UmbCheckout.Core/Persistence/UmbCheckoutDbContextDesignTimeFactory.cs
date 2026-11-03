using Microsoft.EntityFrameworkCore.Design;
using UmbCheckout.Shared.Persistence;

namespace UmbCheckout.Core.Persistence;

internal sealed class UmbCheckoutDbContextDesignTimeFactory : IDesignTimeDbContextFactory<UmbCheckoutDbContext>
{
    public UmbCheckoutDbContext CreateDbContext(string[] args) => new(DesignTimeOptions.For<UmbCheckoutDbContext>(UmbCheckoutDbContext.HistoryTable));
}
