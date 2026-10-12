using UmbCheckout.Shared.Notifications;
using Xunit;

namespace UmbCheckout.Tests.Notifications;

public sealed class UmbCheckoutHooksTests
{
    [Fact]
    public async Task Suppression_flows_into_awaited_calls_and_ends_with_its_scope()
    {
        Assert.False(UmbCheckoutHooks.LoadedSuppressed);

        using (UmbCheckoutHooks.SuppressLoaded())
        {
            Assert.True(await Task.Run(() => UmbCheckoutHooks.LoadedSuppressed));
            Assert.False(UmbCheckoutHooks.SavingSuppressed);
        }

        Assert.False(UmbCheckoutHooks.LoadedSuppressed);
    }

    [Fact]
    public void Nested_scopes_restore_the_outer_state()
    {
        using (UmbCheckoutHooks.SuppressSaving())
        {
            using (UmbCheckoutHooks.SuppressSaving())
            {
            }

            Assert.True(UmbCheckoutHooks.SavingSuppressed);
        }

        Assert.False(UmbCheckoutHooks.SavingSuppressed);
    }
}
