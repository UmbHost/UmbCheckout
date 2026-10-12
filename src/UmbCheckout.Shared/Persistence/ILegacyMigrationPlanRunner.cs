using Umbraco.Cms.Infrastructure.Migrations;

namespace UmbCheckout.Shared.Persistence;

/// <summary>Runs a frozen pre-v17 NPoco migration plan to its end (only on installs that started it).</summary>
public interface ILegacyMigrationPlanRunner
{
    Task RunAsync(MigrationPlan plan, CancellationToken cancellationToken = default);
}
