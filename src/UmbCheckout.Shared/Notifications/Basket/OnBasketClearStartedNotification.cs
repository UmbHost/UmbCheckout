using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Shared.Notifications.Basket;

/// <summary>
/// Notification which is triggered before the Basket is cleared
/// </summary>
public class OnBasketClearStartedNotification : INotification
{
    public Models.Basket Basket { get; set; }

    public OnBasketClearStartedNotification(Models.Basket basket)
    {
        Basket = basket;
    }
}