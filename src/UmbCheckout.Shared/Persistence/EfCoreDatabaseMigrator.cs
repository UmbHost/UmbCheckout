using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace UmbCheckout.Shared.Persistence;

/// <summary>
/// Brings one context's tables up to date at startup. A legacy install (its NPoco plan has a state key) first finishes that plan,
/// then has Baseline recorded as applied; every install then runs the EF migrations.
/// It works on a context from <see cref="IDbContextFactory{TContext}"/>, outside any Umbraco scope: on SQLite (Cache=Shared) EF checks
/// for the database on a second connection, which a schema change still uncommitted in a shared scope transaction would lock out.
/// </summary>
public abstract class EfCoreDatabaseMigrator<TContext> : INotificationAsyncHandler<UmbracoApplicationStartingNotification>
    where TContext : DbContext
{
    private readonly IDbContextFactory<TContext> _contextFactory;
    private readonly IRuntimeState _runtimeState;
    private readonly IKeyValueService _keyValueService;
    private readonly ILegacyMigrationPlanRunner _legacyPlanRunner;
    private readonly ILogger _logger;

    protected EfCoreDatabaseMigrator(
        IDbContextFactory<TContext> contextFactory,
        IRuntimeState runtimeState,
        IKeyValueService keyValueService,
        ILegacyMigrationPlanRunner legacyPlanRunner,
        ILogger logger)
    {
        _contextFactory = contextFactory;
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
            await using var adoptContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
            await BaselineAdoption.AdoptAsync(adoptContext, cancellationToken);
        }

        await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);
        await db.Database.MigrateAsync(cancellationToken);
    }
}
