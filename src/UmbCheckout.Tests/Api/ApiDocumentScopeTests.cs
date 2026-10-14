using UmbCheckout.Core.ManagementApi;
using Xunit;

namespace UmbCheckout.Tests.Api
{
    public class ApiDocumentScopeTests
    {
        private readonly UmbCheckoutSchemaIdHandler _stripeHandler = new("UmbCheckout.Stripe", excludedNamespacePrefix: "UmbCheckout.Stripe.Extras");

        [Fact]
        public void StripeDocument_HandlesStripeModels()
            => Assert.True(_stripeHandler.CanHandle(typeof(UmbCheckout.Stripe.ScopeFixtures.StripeModel)));

        [Fact]
        public void StripeDocument_SkipsExtrasModels()
            => Assert.False(_stripeHandler.CanHandle(typeof(UmbCheckout.Stripe.Extras.ScopeFixtures.ExtrasModel)));

        private readonly UmbCheckoutSchemaIdHandler _extrasHandler = new("UmbCheckout.Stripe.Extras");

        [Fact]
        public void ExtrasDocument_HandlesCursorPageOfExtrasModel()
            => Assert.True(_extrasHandler.CanHandle(typeof(UmbCheckout.Core.ManagementApi.CursorPagedViewModel<UmbCheckout.Stripe.Extras.ScopeFixtures.ExtrasModel>)));

        [Fact]
        public void CursorPagedSchemaIds_AreUniquePerItemType()
        {
            // Mirrors Umbraco's PagedViewModel<T> -> "Paged{T}" naming; one shared ID makes swagger generation throw.
            Assert.Equal("CursorPagedExtrasModel", _extrasHandler.Handle(typeof(UmbCheckout.Core.ManagementApi.CursorPagedViewModel<UmbCheckout.Stripe.Extras.ScopeFixtures.ExtrasModel>)));
            Assert.Equal("CursorPagedOtherExtrasModel", _extrasHandler.Handle(typeof(UmbCheckout.Core.ManagementApi.CursorPagedViewModel<UmbCheckout.Stripe.Extras.ScopeFixtures.OtherExtrasModel>)));
        }

        [Fact]
        public void StripeDocument_SkipsNamespacesThatOnlySharePrefixText()
            => Assert.False(_stripeHandler.CanHandle(typeof(UmbCheckout.StripeX.ScopeFixtures.LookalikeModel)));
    }
}

namespace UmbCheckout.Stripe.ScopeFixtures
{
    public class StripeModel { }
}

namespace UmbCheckout.Stripe.Extras.ScopeFixtures
{
    public class ExtrasModel { }
    public class OtherExtrasModel { }
}

namespace UmbCheckout.StripeX.ScopeFixtures
{
    public class LookalikeModel { }
}
