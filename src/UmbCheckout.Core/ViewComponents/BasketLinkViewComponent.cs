using Microsoft.AspNetCore.Mvc;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.ViewModels;
using UmbCheckout.Shared.Enums;
using UmbCheckout.Shared.Extensions;
using Umbraco.Cms.Core.Models.PublishedContent;
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
        private readonly ICurrencyService _currencyService;

        public BasketLinkViewComponent(IBasketService basketService, IUmbracoContextAccessor umbracoContextAccessor, ICurrencyService currencyService)
        {
            _basketService = basketService;
            _umbracoContextAccessor = umbracoContextAccessor;
            _currencyService = currencyService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string basketAlias = "basket", string linkName = "Basket", string? linkCssClass = null, BasketLinkType linkType = BasketLinkType.TotalCount, Guid? currencyNodeKey = null)
        {
            // The subtotal is shown in the basket's own currency (what it is charged in); the configured chain only applies to an
            // empty basket saved before baskets were per currency.
            var cancellationToken = HttpContext?.RequestAborted ?? CancellationToken.None;
            var basket = await _basketService.Get(cancellationToken);
            var currencyCode = basket.CurrencyCode ?? basket.LineItems.SingleCurrency() ?? await _currencyService.GetCurrencyAsync(currencyNodeKey, cancellationToken);

            var model = new BasketLinkViewModel
            {
                LinkCssClass = linkCssClass,
                LinkName = linkName,
                TotalItems = await _basketService.TotalItems(cancellationToken),
                SubTotal = await _basketService.SubTotal(cancellationToken),
                LinkType = linkType,
                CurrencyCode = currencyCode
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
