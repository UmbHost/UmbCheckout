using Microsoft.Extensions.DependencyInjection;
using Moq;
using Stripe.Checkout;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Composers;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Services;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Webhooks;
using Xunit;

namespace UmbCheckout.Tests.Composers;

/// <summary>#7: a site's own shipping calculator replaces the default, whichever composer runs first.</summary>
public sealed class ShippingOptionsComposerTests
{
    private sealed class SitesOwn : IStripeShippingOptionsService
    {
        public Task<List<SessionShippingOptionOptions>> GetShippingOptions(Basket basket) => GetShippingOptions(basket, CancellationToken.None);

        public Task<List<SessionShippingOptionOptions>> GetShippingOptions(Basket basket, CancellationToken cancellationToken)
            => Task.FromResult(new List<SessionShippingOptionOptions>());
    }

    private static Type Registered(bool? siteFirst)
    {
        var services = new ServiceCollection();
        var builder = new Mock<IUmbracoBuilder>();
        builder.SetupGet(b => b.Services).Returns(services);
        builder.Setup(b => b.WithCollectionBuilder<WebhookEventCollectionBuilder>()).Returns(new WebhookEventCollectionBuilder());

        if (siteFirst == true)
        {
            services.AddTransient<IStripeShippingOptionsService, SitesOwn>();
        }

        new RegisterServicesComposer().Compose(builder.Object);
        if (siteFirst == false)
        {
            services.AddTransient<IStripeShippingOptionsService, SitesOwn>();
        }

        return services.Last(d => d.ServiceType == typeof(IStripeShippingOptionsService)).ImplementationType.Required();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void The_sites_calculator_is_used(bool siteFirst)
        => Assert.Equal(typeof(SitesOwn), Registered(siteFirst));

    [Fact]
    public void Without_one_the_default_is_used()
        => Assert.Equal(typeof(StripeDefaultShippingOptionsService), Registered(null));
}
