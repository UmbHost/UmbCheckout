using System.Net;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using UmbCheckout.Stripe.Controllers.Api;
using UmbCheckout.Stripe.Interfaces;
using Umbraco.Cms.Core.Events;
using Xunit;

namespace UmbCheckout.Tests.Api;

/// <summary>
/// Quality Phase 6: the webhook controller left the obsolete UmbracoApiController. Stripe endpoints are already configured with
/// /umbraco/api/stripewebhookapi/checkoutevents, so that URL must still reach the action, anonymously.
/// </summary>
public sealed class StripeWebhookRouteTests
{
    [Fact]
    public void The_controller_is_a_plain_ControllerBase()
        => Assert.Equal(typeof(ControllerBase), typeof(StripeWebhookApiController).BaseType);

    [Theory]
    [InlineData("/umbraco/api/stripewebhookapi/checkoutevents")]
    [InlineData("/umbraco/api/StripeWebhookApi/CheckoutEvents")]
    public async Task The_webhook_url_is_unchanged_and_needs_no_login(string url)
    {
        var secrets = new Mock<IStripeWebhookSecretProvider>();
        secrets.Setup(p => p.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync((string.Empty, false));
        using var host = await new HostBuilder()
            .ConfigureWebHost(web => web
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddSingleton(secrets.Object);
                    services.AddSingleton(Mock.Of<IEventAggregator>());
                    services.AddSingleton(Mock.Of<IStripeWebhookEventStore>());
                    services.AddLogging();
                    services.AddAuthorization();
                    services.AddControllers()
                        .ConfigureApplicationPartManager(parts =>
                        {
                            parts.ApplicationParts.Clear();
                            parts.FeatureProviders.Add(new OnlyWebhookController());
                        });
                })
                .Configure(app => app
                    .UseRouting()
                    .UseAuthorization()
                    .UseEndpoints(endpoints => endpoints.MapControllers())))
            .StartAsync();

        var response = await host.GetTestClient().PostAsync(url, new StringContent("{}", Encoding.UTF8, "application/json"));

        // Reached the action: with no webhook secret configured it answers 500 so Stripe retries (not 404, 401 or 405).
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        secrets.Verify(p => p.GetAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task The_webhook_is_routed_on_a_website_only_host_without_duplicating_it_on_a_full_host()
    {
        // Phase 6 review: attribute routes need MapControllers(), which in Umbraco 17 only the Management and Delivery APIs call.
        // A front-end-only host (AddCore().AddWebsite(), the public servers of a load-balanced shop) would answer Stripe with 404.
        var services = new ServiceCollection();
        var builder = new Mock<Umbraco.Cms.Core.DependencyInjection.IUmbracoBuilder>();
        builder.SetupGet(b => b.Services).Returns(services);
        new UmbCheckout.Stripe.Composers.StripeWebhookRoutingComposer().Compose(builder.Object);
        var filters = services.BuildServiceProvider().GetRequiredService<Microsoft.Extensions.Options.IOptions<Umbraco.Cms.Web.Common.ApplicationBuilder.UmbracoPipelineOptions>>().Value.PipelineFilters;
        var filter = Assert.Single(filters);

        foreach (var fullHost in new[] { false, true })
        {
            var secrets = new Mock<IStripeWebhookSecretProvider>();
            secrets.Setup(p => p.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync((string.Empty, false));
            using var host = await new HostBuilder()
                .ConfigureWebHost(web => web
                    .UseTestServer()
                    .ConfigureServices(s =>
                    {
                        s.AddSingleton(secrets.Object);
                        s.AddSingleton(Mock.Of<IEventAggregator>());
                        s.AddSingleton(Mock.Of<IStripeWebhookEventStore>());
                        s.AddLogging();
                        s.AddAuthorization();
                        s.AddControllers().ConfigureApplicationPartManager(parts =>
                        {
                            parts.ApplicationParts.Clear();
                            parts.FeatureProviders.Add(new OnlyWebhookController());
                        });
                    })
                    .Configure(app =>
                    {
                        app.UseRouting().UseAuthorization();
                        if (fullHost)
                        {
                            app.UseEndpoints(e => e.MapControllers()); // what the Management API does on a backoffice host
                        }

                        filter.OnEndpoints(app);
                    }))
                .StartAsync();

            var response = await host.GetTestClient().PostAsync("/umbraco/api/stripewebhookapi/checkoutevents", new StringContent("{}", Encoding.UTF8, "application/json"));

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode); // reached the action once (an ambiguous match would throw before it)
            secrets.Verify(p => p.GetAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    /// <summary>A site's own webhook controller: a subclass of the public one.</summary>
    [Route("shop/stripe-webhook")]
    public sealed class SiteWebhookWithItsOwnRoute(ILogger<StripeWebhookApiController> logger, IEventAggregator eventAggregator,
        IStripeWebhookSecretProvider secretProvider, IStripeWebhookEventStore eventStore, SubclassHits hits)
        : StripeWebhookApiController(logger, eventAggregator, secretProvider, eventStore)
    {
        public SubclassHits Hits { get; } = hits.Hit();
    }

    /// <summary>A site's own webhook controller that keeps the package's URL.</summary>
    public sealed class SiteWebhookOnTheSameUrl(ILogger<StripeWebhookApiController> logger, IEventAggregator eventAggregator,
        IStripeWebhookSecretProvider secretProvider, IStripeWebhookEventStore eventStore, SubclassHits hits)
        : StripeWebhookApiController(logger, eventAggregator, secretProvider, eventStore)
    {
        public SubclassHits Hits { get; } = hits.Hit();
    }

    public sealed class SubclassHits
    {
        public int Count { get; private set; }

        public SubclassHits Hit()
        {
            Count++;
            return this;
        }
    }

    private static async Task<IHost> Host(Mock<IStripeWebhookSecretProvider> secrets, SubclassHits hits, params Type[] controllers)
        => await new HostBuilder()
            .ConfigureWebHost(web => web
                .UseTestServer()
                .ConfigureServices(s =>
                {
                    s.AddSingleton(secrets.Object);
                    s.AddSingleton(hits);
                    s.AddSingleton(Mock.Of<IEventAggregator>());
                    s.AddSingleton(Mock.Of<IStripeWebhookEventStore>());
                    s.AddLogging();
                    s.AddAuthorization();
                    s.AddControllers().ConfigureApplicationPartManager(parts =>
                    {
                        parts.ApplicationParts.Clear();
                        parts.FeatureProviders.Add(new Only(controllers));
                    });
                    var builder = new Mock<Umbraco.Cms.Core.DependencyInjection.IUmbracoBuilder>();
                    builder.SetupGet(b => b.Services).Returns(s);
                    new UmbCheckout.Stripe.Composers.StripeWebhookRoutingComposer().Compose(builder.Object);
                })
                .Configure(app => app.UseRouting().UseAuthorization().UseEndpoints(e => e.MapControllers())))
            .StartAsync();

    private static Mock<IStripeWebhookSecretProvider> NoSecret()
    {
        var secrets = new Mock<IStripeWebhookSecretProvider>();
        secrets.Setup(p => p.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync((string.Empty, false));
        return secrets;
    }

    private static Task<HttpResponseMessage> Post(IHost host, string url)
        => host.GetTestClient().PostAsync(url, new StringContent("{}", Encoding.UTF8, "application/json"));

    [Fact]
    public async Task A_subclass_with_its_own_route_is_the_one_webhook_endpoint()
    {
        // Review minor: a subclass inherited [Route] as well as declaring its own, so the package URL matched two actions (AmbiguousMatch 500).
        var secrets = NoSecret();
        var hits = new SubclassHits();
        using var host = await Host(secrets, hits, typeof(StripeWebhookApiController), typeof(SiteWebhookWithItsOwnRoute));

        Assert.Equal(HttpStatusCode.InternalServerError, (await Post(host, "/shop/stripe-webhook/checkoutevents")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Post(host, "/umbraco/api/stripewebhookapi/checkoutevents")).StatusCode);

        Assert.Equal(1, hits.Count);
        secrets.Verify(p => p.GetAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task A_subclass_without_a_route_takes_over_the_package_url()
    {
        var secrets = NoSecret();
        var hits = new SubclassHits();
        using var host = await Host(secrets, hits, typeof(StripeWebhookApiController), typeof(SiteWebhookOnTheSameUrl));

        Assert.Equal(HttpStatusCode.InternalServerError, (await Post(host, "/umbraco/api/stripewebhookapi/checkoutevents")).StatusCode);

        Assert.Equal(1, hits.Count);
    }

    [Fact]
    public async Task Without_a_subclass_the_package_url_is_unchanged()
    {
        var secrets = NoSecret();
        using var host = await Host(secrets, new SubclassHits(), typeof(StripeWebhookApiController));

        Assert.Equal(HttpStatusCode.InternalServerError, (await Post(host, "/umbraco/api/stripewebhookapi/checkoutevents")).StatusCode);
        secrets.Verify(p => p.GetAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class Only(Type[] controllers) : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var controller in controllers)
            {
                feature.Controllers.Add(controller.GetTypeInfo());
            }
        }
    }

    private sealed class OnlyWebhookController : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
            => feature.Controllers.Add(typeof(StripeWebhookApiController).GetTypeInfo());
    }
}
