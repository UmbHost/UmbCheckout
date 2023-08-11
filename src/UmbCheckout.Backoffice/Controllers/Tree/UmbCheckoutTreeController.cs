using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UmbCheckout.Shared;
using UmbHost.Licensing.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;

namespace UmbCheckout.Backoffice.Controllers.Tree
{
    /// <summary>
    /// Umbraco TreeController which adds the group and root menu link
    /// </summary>
    [Tree("settings", Consts.TreeAlias, TreeTitle = Consts.PackageName, TreeGroup = Consts.TreeGroup, IsSingleNodeTree = true, SortOrder = 35)]
    [PluginController(Consts.PackageName)]
    public class UmbCheckoutTreeController : TreeController
    {

        private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;
        private readonly ILocalizedTextService _localizedTextService;

        public UmbCheckoutTreeController(ILocalizedTextService localizedTextService, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, IEventAggregator eventAggregator, IMenuItemCollectionFactory menuItemCollectionFactory, LicenseService licenseService) 
            : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
            _localizedTextService = localizedTextService;
            _menuItemCollectionFactory = menuItemCollectionFactory ?? throw new ArgumentNullException(nameof(menuItemCollectionFactory));
            licenseService.RunLicenseCheck();
        }

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                nodes.Add(CreateTreeNode("1", "-1", queryStrings, _localizedTextService.Localize(Consts.LocalizationKeys.Area, Consts.LocalizationKeys.Configuration, CultureInfo.CurrentUICulture), "icon-settings", false, $"{Constants.Applications.Settings}/{"UmbCheckout"}/{"dashboard"}"));
            }

            return nodes;
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        {
            var menu = _menuItemCollectionFactory.Create();
            return menu;
        }

        protected override ActionResult<TreeNode?> CreateRootNode(FormCollection queryStrings)
        {
            var rootResult = base.CreateRootNode(queryStrings);
            if (rootResult.Result is not null)
            {
                return rootResult;
            }

            var root = rootResult.Value;

            if (root != null)
            {
                root.RoutePath = $"{Constants.Applications.Settings}/UmbCheckout/dashboard";

                root.Icon = "icon-shopping-basket-alt-2";
                root.HasChildren = true;
                root.MenuUrl = null;
            }

            return root;
        }
    }
}
