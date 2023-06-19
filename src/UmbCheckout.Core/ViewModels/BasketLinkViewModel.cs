namespace UmbCheckout.Core.ViewModels
{
    public class BasketLinkViewModel
    {
        public string LinkName { get; set; } = string.Empty;

        public string LinkUrl { get; set; } = string.Empty;

        public long TotalItems { get; set; }
    }
}
