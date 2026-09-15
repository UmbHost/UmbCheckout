using UmbCheckout.Core.Migrations;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;

namespace UmbCheckout.Core.NotificationHandlers
{
    /// <summary>
    /// Handles the migration of the Configuration table
    /// </summary>
    internal class RunUmbCheckoutConfigurationMigration : INotificationAsyncHandler<UmbracoApplicationStartingNotification>
    {
        private readonly IMigrationPlanExecutor _migrationPlanExecutor;
        private readonly ICoreScopeProvider _coreScopeProvider;
        private readonly IKeyValueService _keyValueService;
        private readonly IRuntimeState _runtimeState;

        public RunUmbCheckoutConfigurationMigration(IMigrationPlanExecutor migrationPlanExecutor, ICoreScopeProvider coreScopeProvider, IKeyValueService keyValueService, IRuntimeState runtimeState)
        {
            _migrationPlanExecutor = migrationPlanExecutor;
            _coreScopeProvider = coreScopeProvider;
            _keyValueService = keyValueService;
            _runtimeState = runtimeState;
        }

        public async Task HandleAsync(UmbracoApplicationStartingNotification notification, CancellationToken cancellationToken)
        {
            if (_runtimeState.Level < RuntimeLevel.Run)
            {
                return;
            }

            var migrationPlan = new MigrationPlan("UmbCheckoutConfiguration");
            migrationPlan.From(string.Empty)
                .To<AddUmbCheckoutConfigurationTable>("42d30e38-1cf0-4bd8-8f47-c6517e4be09d");
            migrationPlan.From("42d30e38-1cf0-4bd8-8f47-c6517e4be09d")
                .To<AddUmbCheckoutKey>("a877baa5-644d-4367-a1c0-5dccda876f79");
            migrationPlan.From("a877baa5-644d-4367-a1c0-5dccda876f79")
                .To<AddUmbCheckoutCurrencyCode>("943bb809-80f8-4435-8c94-c6717e54a808");
            migrationPlan.From("943bb809-80f8-4435-8c94-c6717e54a808")
                .To<SetMetaDataPropertyEditorUiAlias>("3b1f7d5e-6c2a-4f0e-9a8d-2e5b7c4d1f90");

            var upgrader = new Upgrader(migrationPlan);
            await upgrader.ExecuteAsync(
                _migrationPlanExecutor,
                _coreScopeProvider,
                _keyValueService);
        }
    }
}
