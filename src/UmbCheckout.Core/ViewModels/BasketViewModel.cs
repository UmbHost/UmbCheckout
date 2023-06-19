using UmbCheckout.Shared.Models;

namespace UmbCheckout.Core.ViewModels
{
    /// <summary>
    /// View model for the Basket View Component
    /// </summary>
    public class BasketViewModel
    {
        public Basket Basket { get; set; } = new Basket();

        public decimal SubTotal { get; set; }
        public long TotalItems { get; set; }
    }
}
