using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Persistence.EFCore.Scoping;

namespace UmbCheckout.Shared.Persistence;

/// <summary>
/// Brings one context's tables up to date at startup. A legacy install (its NPoco plan has a state key) first finishes that plan,
/// then has Baseline recorded as applied; every install then runs the EF migrations.
/// </summary>
public abstract class EfCoreDatabaseMigrator<TContext> : INotificationAsyncHandler<UmbracoApplicationStartingNotification>
    where TContext : DbContext
{
    private readonly IEFCoreScopeProvider<TContext> _scopeProvider;
    private readonly IRuntimeState _runtimeState;
    private readonly IKeyValueService _keyValueService;
    private readonly ILegacyMigrationPlanRunner _legacyPlanRunner;
    private readonly ILogger _logger;

    protected EfCoreDatabaseMigrator(
        IEFCoreScopeProvider<TContext> scopeProvider,
        IRuntimeState runtimeState,
        IKeyValueService keyValueService,
        ILegacyMigrationPlanRunner legacyPlanRunner,
        ILogger logger)
    {
        _scopeProvider = scopeProvider;
        _runtimeState = runtimeState;
        _keyValueService = keyValueService;
        _legacyPlanRunner = legacyPlanRunner;
        _logger = logger;
    }

    /// <summary>The NPoco plan that created these tables before v17, or null.</summary>
    protected abstract MigrationPlan? LegacyPlan { get; }

    /// <summary>
    /// Whether this context's tables belong on this site. Paid add-ons override it with a licence check; it runs when the
    /// handler runs (never in a constructor, which runs before the licensing table is migrated).
    /// </summary>
    protected virtual bool ShouldMigrate() => true;

    public async Task HandleAsync(UmbracoApplicationStartingNotification notification, CancellationToken cancellationToken)
    {
        if (_runtimeState.Level < RuntimeLevel.Run)
        {
            return;
        }

        try
        {
            await MigrateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Migrating the {DbContext} tables failed", typeof(TContext).Name);
            throw;
        }
    }

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        if (!ShouldMigrate())
        {
            return;
        }

        if (LegacyPlan is { } plan && _keyValueService.GetValue(Constants.Conventions.Migrations.KeyValuePrefix + plan.Name) is not null)
        {
            await _legacyPlanRunner.RunAsync(plan, cancellationToken);
            using var adoptScope = _scopeProvider.CreateScope();
            await adoptScope.ExecuteWithContextAsync(db => BaselineAdoption.AdoptAsync(db, cancellationToken));
            adoptScope.Complete();
        }

        using var scope = _scopeProvider.CreateScope();
        await scope.ExecuteWithContextAsync<Task>(db => db.Database.MigrateAsync(cancellationToken));
        scope.Complete();
    }
}
