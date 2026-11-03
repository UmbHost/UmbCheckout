using Microsoft.Extensions.Logging;
using UmbCheckout.Shared.Persistence;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace UmbCheckout.Core.Migrations.Setup;

/// <summary>Section access and the MetaData data-type move every install needs, fresh or upgraded. Each step is safe to run again.</summary>
internal sealed class UmbCheckoutSetupMigrator(IRuntimeState runtimeState, ILegacyMigrationPlanRunner planRunner, ILogger<UmbCheckoutSetupMigrator> logger)
    : INotificationAsyncHandler<UmbracoApplicationStartingNotification>
{
    internal static MigrationPlan CreatePlan()
    {
        var plan = new MigrationPlan("UmbCheckoutSetup");
        plan.From(string.Empty).To<AddUmbCheckoutSectionAccess>("57aa6ddc-e4c6-4262-ba73-51daa2cbf6dc");
        plan.From("57aa6ddc-e4c6-4262-ba73-51daa2cbf6dc").To<MigrateMetaDataToPlainJson>("0cbc2a44-97e9-479d-8caf-1d5cd440b1d1");
        return plan;
    }

    public async Task HandleAsync(UmbracoApplicationStartingNotification notification, CancellationToken cancellationToken)
    {
        if (runtimeState.Level < RuntimeLevel.Run)
        {
            return;
        }

        try
        {
            await planRunner.RunAsync(CreatePlan(), cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "The UmbCheckout setup plan failed");
            throw;
        }
    }
}
