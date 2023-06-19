using Microsoft.AspNetCore.Mvc;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.ViewModels;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common;
using Umbraco.Extensions;

namespace UmbCheckout.Core.ViewComponents
{
    /// <summary>
    /// A view component for the Basket link with total item count
    /// </summary>
    [ViewComponent(Name = "BasketLink")]
    public class BasketLinkViewComponent : ViewComponent
    {
        private readonly IBasketService _basketService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public BasketLinkViewComponent(IBasketService basketService, IUmbracoContextAccessor umbracoContextAccessor)
        {
            _basketService = basketService;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync(string basketAlias = "basket", string linkName = "Basket Count")
        {
            var model = new BasketLinkViewModel
            {
                LinkName = linkName,
                TotalItems = await _basketService.TotalItems()
            };

            var hasContext = _umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext);
            if (hasContext)
            {
                var basketNode = umbracoContext?.PublishedRequest?.PublishedContent?.Root()?.DescendantOfType(basketAlias);
                model.LinkUrl = basketNode != null ? basketNode.Url() : "#";
            }

            return View("~/Views/Partials/UmbCheckout/_BasketLink.cshtml", model);
        }
    }
}
