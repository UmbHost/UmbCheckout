using Microsoft.AspNetCore.Mvc;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.ViewModels;

namespace UmbCheckout.Core.ViewComponents
{
    /// <summary>
    /// View component for the basket
    /// </summary>
    [ViewComponent(Name = "Basket")]
    public class BasketViewComponent : ViewComponent
    {
        private readonly IBasketService _basketService;

        public BasketViewComponent(IBasketService basketService)
        {
            _basketService = basketService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var basket = await _basketService.Get();

            var model = new BasketViewModel
            {
                Basket = basket,
                SubTotal = await _basketService.SubTotal(),
                TotalItems = await _basketService.TotalItems()
            };

            return View("~/Views/Partials/UmbCheckout/_Basket.cshtml", model);
        }
    }
}
