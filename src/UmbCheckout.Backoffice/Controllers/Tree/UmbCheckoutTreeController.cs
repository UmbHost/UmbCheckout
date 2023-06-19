using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using UmbHost.Licensing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Trees;
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
    [Tree("settings", "umbCheckout", TreeTitle = "UmbCheckout", TreeGroup = "umbCheckout", IsSingleNodeTree = true, SortOrder = 35)]
    [PluginController("UmbCheckout")]
    [LicenseProvider(typeof(UmbLicensingProvider))]
    public class UmbCheckoutTreeController : TreeController
    {

        private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;
        private readonly bool _licenseIsValid;

        public UmbCheckoutTreeController(ILocalizedTextService localizedTextService, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, IEventAggregator eventAggregator, IMenuItemCollectionFactory menuItemCollectionFactory) 
            : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
            _menuItemCollectionFactory = menuItemCollectionFactory ?? throw new ArgumentNullException(nameof(menuItemCollectionFactory));
            _licenseIsValid = LicenseManager.IsValid(typeof(UmbCheckoutTreeController));
        }

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                nodes.Add(CreateTreeNode("1", "-1", queryStrings, "Configuration", "icon-settings", false, $"{Constants.Applications.Settings}/{"umbCheckout"}/{"dashboard"}"));
            }

            return nodes;
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        {
            var menu = _menuItemCollectionFactory.Create();

            if (_licenseIsValid && id == "2")
            {
                MenuItem? item = menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true, useLegacyIcon: false);
                item?.NavigateToRoute($"{Constants.Applications.Settings}/umbCheckout/taxRate");
            }

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
                root.RoutePath = $"{Constants.Applications.Settings}/umbCheckout/dashboard";

                root.Icon = "icon-shopping-basket-alt-2";
                root.HasChildren = true;
                root.MenuUrl = null;
            }

            return root;
        }
    }
}
