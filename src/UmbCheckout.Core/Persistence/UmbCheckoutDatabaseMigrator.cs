using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UmbCheckout.Core.Migrations;
using UmbCheckout.Core.Migrations.Legacy;
using UmbCheckout.Shared.Persistence;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace UmbCheckout.Core.Persistence;

internal sealed class UmbCheckoutDatabaseMigrator(
    IDbContextFactory<UmbCheckoutDbContext> contextFactory,
    IRuntimeState runtimeState,
    IKeyValueService keyValueService,
    ILegacyMigrationPlanRunner legacyPlanRunner,
    ILogger<UmbCheckoutDatabaseMigrator> logger)
    : EfCoreDatabaseMigrator<UmbCheckoutDbContext>(contextFactory, runtimeState, keyValueService, legacyPlanRunner, logger)
{
    protected override MigrationPlan LegacyPlan => CreateLegacyPlan();

    /// <summary>The frozen pre-v17 plan, step for step (state ids unchanged).</summary>
    internal static MigrationPlan CreateLegacyPlan()
    {
        var plan = new MigrationPlan("UmbCheckoutConfiguration");
        plan.From(string.Empty).To<AddUmbCheckoutConfigurationTable>("42d30e38-1cf0-4bd8-8f47-c6517e4be09d");
        plan.From("42d30e38-1cf0-4bd8-8f47-c6517e4be09d").To<AddUmbCheckoutKey>("a877baa5-644d-4367-a1c0-5dccda876f79");
        plan.From("a877baa5-644d-4367-a1c0-5dccda876f79").To<AddUmbCheckoutCurrencyCode>("943bb809-80f8-4435-8c94-c6717e54a808");
        plan.From("943bb809-80f8-4435-8c94-c6717e54a808").To<AddUmbCheckoutSectionAccess>("7b1c6f5e-2d4a-4c8e-9f3a-1e5d6c7b8a90");
        plan.From("7b1c6f5e-2d4a-4c8e-9f3a-1e5d6c7b8a90").To<MigrateMetaDataToPlainJson>("4e8a2c1d-6b5f-4a3e-8d7c-9f0e1a2b3c4d");
        return plan;
    }
}
