using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket
{
    /// <summary>
    /// Notification which is triggered before multiple line items are removed from the Basket
    /// </summary>
    public class OnBasketRemoveManyStartedNotification : INotification
    {
        public Models.Basket Basket { get; }
        public IEnumerable<Guid> Keys { get; }

        public OnBasketRemoveManyStartedNotification(IEnumerable<Guid> keys, Models.Basket basket)
        {
            Keys = keys;
            Basket = basket;
        }
    }
}
