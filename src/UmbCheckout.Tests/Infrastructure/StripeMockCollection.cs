using UmbCheckout.Testing;
using Xunit;

namespace UmbCheckout.Tests.Infrastructure
{
    /// <summary>One stripe-mock container (<see cref="StripeMockFixture"/>, in UmbCheckout.Testing) for this assembly's tests.</summary>
    [CollectionDefinition("stripe-mock")]
    public sealed class StripeMockCollection : ICollectionFixture<StripeMockFixture>
    {
    }
}
