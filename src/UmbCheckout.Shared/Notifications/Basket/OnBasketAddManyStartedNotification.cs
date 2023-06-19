using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket
{
    public class OnBasketAddManyStartedNotification : INotification
    {
        public IEnumerable<LineItem> LineItem { get; }

        public OnBasketAddManyStartedNotification(IEnumerable<LineItem> lineItem)
        {
            LineItem = lineItem;
        }
    }
}
