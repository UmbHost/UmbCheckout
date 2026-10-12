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

    private sealed class OnlyWebhookController : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
            => feature.Controllers.Add(typeof(StripeWebhookApiController).GetTypeInfo());
    }
}
