using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;

namespace UmbCheckout.Core.Migrations
{
    /// <summary>
    /// Ensures the UmbCheckout user group exists and that Administrators can see the umbCheckout section. Idempotent.
    /// </summary>
    internal class AddUmbCheckoutSectionAccess : AsyncMigrationBase
    {
        private const string SectionAlias = "umbCheckout";
        private const string GroupAlias = "umbCheckout";
        private readonly IUserGroupService _userGroupService;
        private readonly IShortStringHelper _shortStringHelper;

        public AddUmbCheckoutSectionAccess(IMigrationContext context, IUserGroupService userGroupService, IShortStringHelper shortStringHelper) : base(context)
        {
            _userGroupService = userGroupService;
            _shortStringHelper = shortStringHelper;
        }

        protected override async Task MigrateAsync()
        {
            if (await _userGroupService.GetAsync(GroupAlias) is null)
            {
                var group = new UserGroup(_shortStringHelper, 0, GroupAlias, "UmbCheckout", "icon-shopping-basket-alt-2");
                group.AddAllowedSection(SectionAlias);
                var created = await _userGroupService.CreateAsync(group, Constants.Security.SuperUserKey);
                if (!created.Success)
                {
                    Logger.LogWarning("UmbCheckout user group could not be created: {Status}", created.Status);
                }
            }

            var admin = await _userGroupService.GetAsync(Constants.Security.AdminGroupAlias);
            if (admin is not null && !admin.AllowedSections.Contains(SectionAlias))
            {
                admin.AddAllowedSection(SectionAlias);
                var updated = await _userGroupService.UpdateAsync(admin, Constants.Security.SuperUserKey);
                if (!updated.Success)
                {
                    Logger.LogWarning("UmbCheckout section could not be granted to Administrators: {Status}", updated.Status);
                }
            }
        }
    }
}
