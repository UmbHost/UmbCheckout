using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using UmbCheckout.Core.Composers;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Xunit;

namespace UmbCheckout.Tests.Sessions;

public sealed class UmbCheckoutComposerSessionTests
{
    [Fact]
    public void The_session_is_available_while_Umbraco_routes_the_request()
    {
        // The success handler clears the paid basket from RoutingRequestNotification, which Umbraco raises while it routes
        // (inside UseRouting's dynamic route matching). Session middleware added after routing isn't there yet.
        var services = new ServiceCollection();
        var builder = new Mock<IUmbracoBuilder>();
        builder.SetupGet(b => b.Services).Returns(services);

        new UmbCheckoutComposer().Compose(builder.Object);

        var options = services.BuildServiceProvider().GetRequiredService<IOptions<UmbracoPipelineOptions>>().Value;
        var filter = Assert.IsType<UmbracoPipelineFilter>(Assert.Single(options.PipelineFilters, f => f.Name == "UmbCheckout"));
        Assert.NotNull(filter.PreRouting);
        Assert.Null(filter.PostRouting);
    }
}
