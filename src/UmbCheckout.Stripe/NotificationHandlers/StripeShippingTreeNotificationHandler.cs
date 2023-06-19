using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;

namespace UmbCheckout.Stripe.NotificationHandlers
{
    public class StripeShippingTreeNotificationHandler : INotificationHandler<TreeNodesRenderingNotification>
    {
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly UmbracoApiControllerTypeCollection _apiControllers;

        public StripeShippingTreeNotificationHandler(IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, UmbracoApiControllerTypeCollection apiControllers)
        {
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _apiControllers = apiControllers;
        }

        public void Handle(TreeNodesRenderingNotification notification)
        {
            if (notification.TreeAlias.Equals("umbCheckout"))
            {
                var menuItem = CreateTreeNode("3", "-1", notification.QueryString, "Stripe Shipping Rates", "icon-truck", $"{Constants.Applications.Settings}/umbCheckout/shippingrates");

                notification.Nodes.Add(menuItem);
            }
        }

        public TreeNode CreateTreeNode(string id, string parentId, FormCollection queryStrings, string title, string icon, string routePath)
        {
            if (_actionContextAccessor.ActionContext != null)
            {
                var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

                if (_actionContextAccessor.ActionContext.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                {
                    var jsonUrl = urlHelper.GetTreeUrl(_apiControllers, controllerActionDescriptor.ControllerTypeInfo, id, queryStrings);
                    var menuUrl = urlHelper.GetMenuUrl(_apiControllers, controllerActionDescriptor.ControllerTypeInfo, id, queryStrings);
                    return new TreeNode(id, parentId, jsonUrl, menuUrl) { Name = title, RoutePath = routePath, Icon = icon };
                }
            }

            return new TreeNode(id, parentId, string.Empty, string.Empty) { Name = title, RoutePath = routePath, Icon = icon };
        }
    }
}
