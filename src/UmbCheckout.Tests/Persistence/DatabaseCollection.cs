using UmbCheckout.Testing;
using Xunit;

namespace UmbCheckout.Tests.Persistence
{
    /// <summary>One SQL Server container (<see cref="SqlServerContainerFixture"/>, in UmbCheckout.Testing) for this assembly's tests. It starts on first use.</summary>
    [CollectionDefinition(Name)]
    public sealed class DatabaseCollection : ICollectionFixture<SqlServerContainerFixture>
    {
        public const string Name = "Database";
    }
}
