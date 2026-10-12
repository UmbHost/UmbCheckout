using UmbCheckout.Testing;
using Xunit;

namespace UmbCheckout.Tests.Persistence;

[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<SqlServerContainerFixture>
{
    public const string Name = "Database";
}
