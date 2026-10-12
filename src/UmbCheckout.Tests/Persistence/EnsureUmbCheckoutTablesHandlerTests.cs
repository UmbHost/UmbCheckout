using Microsoft.Extensions.DependencyInjection;
using Moq;
using UmbCheckout.Shared.Persistence;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;
using Xunit;

namespace UmbCheckout.Tests.Persistence;

public sealed class EnsureUmbCheckoutTablesHandlerTests
{
    private sealed class ImportStarting : INotification;

    [Fact]
    public async Task Every_registered_migrator_runs_before_the_notification_continues()
    {
        var first = new Mock<IUmbCheckoutTablesMigrator>();
        var second = new Mock<IUmbCheckoutTablesMigrator>();
        var handler = new EnsureUmbCheckoutTablesHandler<ImportStarting>([first.Object, second.Object]);

        await handler.HandleAsync(new ImportStarting(), CancellationToken.None);

        first.Verify(m => m.MigrateAsync(It.IsAny<CancellationToken>()), Times.Once);
        second.Verify(m => m.MigrateAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Registering_it_from_several_packages_adds_it_once()
    {
        // UmbCheckout.uSync, UmbCheckout.Stripe.uSync and the add-ons' uSync each register it
        var builder = new Mock<IUmbracoBuilder>();
        var services = new ServiceCollection();
        builder.SetupGet(b => b.Services).Returns(services);

        builder.Object.EnsureUmbCheckoutTablesOn<ImportStarting>();
        builder.Object.EnsureUmbCheckoutTablesOn<ImportStarting>();

        Assert.Single(services, d => d.ServiceType == typeof(INotificationAsyncHandler<ImportStarting>));
    }
}
