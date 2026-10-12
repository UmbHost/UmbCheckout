using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UmbCheckout.Core.Migrations.Setup;
using UmbCheckout.Core.Persistence;
using UmbCheckout.Shared.Persistence;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace UmbCheckout.Core.Composers
{
    /// <summary>Registers the configuration table's EF Core context and its startup migration. Sites need nothing in Program.cs.</summary>
    internal class PersistenceComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.TryAddTransient<ILegacyMigrationPlanRunner, UpgraderLegacyMigrationPlanRunner>();
            builder.Services.AddUmbracoDbContext<UmbCheckoutDbContext>(
                (_, options, connectionString, providerName) => options.UseUmbCheckoutProvider(providerName, connectionString, UmbCheckoutDbContext.HistoryTable),
                shareUmbracoConnection: true);
            builder.AddNotificationAsyncHandler<UmbracoApplicationStartingNotification, UmbCheckoutDatabaseMigrator>();
            builder.Services.AddTransient<IUmbCheckoutTablesMigrator, UmbCheckoutDatabaseMigrator>();
            builder.AddNotificationAsyncHandler<UmbracoApplicationStartingNotification, UmbCheckoutSetupMigrator>();
        }
    }
}
