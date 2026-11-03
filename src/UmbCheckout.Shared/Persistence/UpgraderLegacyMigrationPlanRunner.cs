using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;

namespace UmbCheckout.Shared.Persistence;

public sealed class UpgraderLegacyMigrationPlanRunner(
    IMigrationPlanExecutor migrationPlanExecutor,
    ICoreScopeProvider coreScopeProvider,
    IKeyValueService keyValueService) : ILegacyMigrationPlanRunner
{
    public async Task RunAsync(MigrationPlan plan, CancellationToken cancellationToken = default)
    {
        var result = await new Upgrader(plan).ExecuteAsync(migrationPlanExecutor, coreScopeProvider, keyValueService);
        if (!result.Successful)
        {
            throw new InvalidOperationException($"The legacy migration plan {plan.Name} failed.", result.Exception);
        }
    }
}
