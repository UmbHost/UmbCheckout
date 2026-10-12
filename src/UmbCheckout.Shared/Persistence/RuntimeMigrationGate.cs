using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace UmbCheckout.Shared.Persistence;

/// <summary>
/// When custom tables can be migrated at startup: once Umbraco's own schema is current. That is a running site, or one whose only pending
/// work is package migrations, which is how a fresh install boots; those package migrations (uSync's first-boot import among them) run
/// after startup and need the tables in place.
/// </summary>
public static class RuntimeMigrationGate
{
    public static bool CanMigrate(IRuntimeState runtimeState) =>
        runtimeState.Level == RuntimeLevel.Run
        || (runtimeState.Level == RuntimeLevel.Upgrade && runtimeState.Reason == RuntimeLevelReason.UpgradePackageMigrations);
}
