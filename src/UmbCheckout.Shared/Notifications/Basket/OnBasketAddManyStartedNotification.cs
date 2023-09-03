using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket
{
    /// <summary>
    /// Notification which is triggered before multiple line items are added to the Basket
    /// </summary>
    public class OnBasketAddManyStartedNotification : INotification
    {
        public IEnumerable<LineItem> LineItem { get; }

        public OnBasketAddManyStartedNotification(IEnumerable<LineItem> lineItem)
        {
            LineItem = lineItem;
        }
    }
}
