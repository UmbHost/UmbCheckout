using Microsoft.AspNetCore.Mvc;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.ViewModels;
using UmbCheckout.Shared.Enums;
using Umbraco.Cms.Core.Web;
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

        public async Task<IViewComponentResult> InvokeAsync(string basketAlias = "basket", string linkName = "Basket", string? linkCssClass = null, BasketLinkType linkType = BasketLinkType.TotalCount)
        {
            var model = new BasketLinkViewModel
            {
                LinkCssClass = linkCssClass,
                LinkName = linkName,
                TotalItems = await _basketService.TotalItems(),
                SubTotal = await _basketService.SubTotal(),
                LinkType = linkType
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
