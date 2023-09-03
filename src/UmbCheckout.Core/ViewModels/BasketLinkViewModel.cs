using UmbCheckout.Shared.Enums;

namespace UmbCheckout.Core.ViewModels
{
    /// <summary>
    /// View model for the Basket Link View Component
    /// </summary>
    public class BasketLinkViewModel
    {

        public string? LinkCssClass { get; set; } = null;

        public string LinkName { get; set; } = string.Empty;

        public string LinkUrl { get; set; } = string.Empty;

        public long TotalItems { get; set; }

        public BasketLinkType LinkType { get; set; } = BasketLinkType.TotalCount;
        public decimal SubTotal { get; set; }
    }
}
