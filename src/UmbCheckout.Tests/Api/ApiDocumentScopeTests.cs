using UmbCheckout.Core.ManagementApi;
using Xunit;

namespace UmbCheckout.Tests.Api
{
    public class ApiDocumentScopeTests
    {
        private readonly UmbCheckoutSchemaIdHandler _stripeHandler = new("UmbCheckout.Stripe", excludedNamespacePrefix: "UmbCheckout.Stripe.Addons");

        [Fact]
        public void StripeDocument_HandlesStripeModels()
            => Assert.True(_stripeHandler.CanHandle(typeof(UmbCheckout.Stripe.ScopeFixtures.StripeModel)));

        [Fact]
        public void StripeDocument_SkipsAddonsModels()
            => Assert.False(_stripeHandler.CanHandle(typeof(UmbCheckout.Stripe.Addons.ScopeFixtures.AddonsModel)));

        [Fact]
        public void StripeDocument_SkipsNamespacesThatOnlySharePrefixText()
            => Assert.False(_stripeHandler.CanHandle(typeof(UmbCheckout.StripeX.ScopeFixtures.LookalikeModel)));
    }
}

namespace UmbCheckout.Stripe.ScopeFixtures
{
    public class StripeModel { }
}

namespace UmbCheckout.Stripe.Addons.ScopeFixtures
{
    public class AddonsModel { }
}

namespace UmbCheckout.StripeX.ScopeFixtures
{
    public class LookalikeModel { }
}
