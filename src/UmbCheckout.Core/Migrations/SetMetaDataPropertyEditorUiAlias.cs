using Microsoft.Extensions.Logging;
using UmbCheckout.Shared;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace UmbCheckout.Core.Migrations
{
    /// <summary>
    /// Umbraco 14+ requires a property editor UI alias on each data type, this sets it for existing Meta Data data types
    /// </summary>
    internal class SetMetaDataPropertyEditorUiAlias : AsyncMigrationBase
    {
        private readonly IDataTypeService _dataTypeService;

        public SetMetaDataPropertyEditorUiAlias(IMigrationContext context, IDataTypeService dataTypeService) : base(context)
        {
            _dataTypeService = dataTypeService;
        }

        protected override async Task MigrateAsync()
        {
            Logger.LogDebug("Running migration {MigrationStep}", "UmbCheckoutSetMetaDataPropertyEditorUiAlias");

            var dataTypes = await _dataTypeService.GetByEditorAliasAsync(Consts.PropertyEditors.MetaDataAlias);
            foreach (var dataType in dataTypes.Where(x => x.EditorUiAlias != Consts.PropertyEditors.MetaDataUiAlias))
            {
                dataType.EditorUiAlias = Consts.PropertyEditors.MetaDataUiAlias;
                await _dataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);
            }
        }
    }
}
