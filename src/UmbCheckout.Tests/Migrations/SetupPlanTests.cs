using UmbCheckout.Core.Migrations;
using UmbCheckout.Core.Migrations.Setup;
using UmbCheckout.Core.Persistence;
using Xunit;

namespace UmbCheckout.Tests.Migrations;

public sealed class SetupPlanTests
{
    [Fact]
    public void The_setup_plan_grants_section_access_then_moves_the_metadata_data_type_with_its_own_state_ids()
    {
        var plan = UmbCheckoutSetupMigrator.CreatePlan();

        Assert.Equal("UmbCheckoutSetup", plan.Name);
        var steps = plan.Transitions.Values.Where(t => t is not null).Select(t => t!.MigrationType).ToList();
        Assert.Equal([typeof(AddUmbCheckoutSectionAccess), typeof(MigrateMetaDataToPlainJson)], steps);
        var legacyStates = UmbCheckoutDatabaseMigrator.CreateLegacyPlan().Transitions.Keys.ToHashSet();
        Assert.DoesNotContain(plan.Transitions.Keys, s => s != string.Empty && legacyStates.Contains(s));
    }
}
