#if NET8_0_OR_GREATER
using UmbCheckout.Stripe.Notifications;
using UmbCheckout.Stripe.Notifications.Webhooks;
using UmbCheckout.Stripe.Webhooks;
using Umbraco.Cms.Core.Webhooks;
using Xunit;

namespace UmbCheckout.Tests.Webhooks;

/// <summary>Each Umbraco webhook event fires on the notification its name says (the shipping-rate saved webhook once fired on deletes). Webhooks exist on Umbraco 13 (net8.0) only.</summary>
public sealed class WebhookNotificationMappingTests
{
    public static TheoryData<Type, Type> Mappings => new()
    {
        { typeof(OnShippingRateSavedWebhook), typeof(OnShippingRateSavedNotification) },
        { typeof(OnShippingRateDeletedWebhook), typeof(OnShippingRateDeletedNotification) },
        { typeof(OnChargeFailedWebhook), typeof(OnChargeFailedNotification) },
        { typeof(OnChargeSucceededWebhook), typeof(OnChargeSucceededNotification) },
        { typeof(OnCheckoutSessionCompletedWebhook), typeof(OnCheckoutSessionCompletedNotification) },
        { typeof(OnCheckoutSessionExpiredWebhook), typeof(OnCheckoutSessionExpiredNotification) },
        { typeof(OnPaymentFailedWebhook), typeof(OnPaymentFailedNotification) },
        { typeof(OnPaymentIntentCancelledWebhook), typeof(OnPaymentIntentCancelledNotification) },
        { typeof(OnPaymentIntentCreatedWebhook), typeof(OnPaymentIntentCreatedNotification) },
        { typeof(OnPaymentIntentPaymentFailedWebhook), typeof(OnPaymentIntentPaymentFailedNotification) },
        { typeof(OnPaymentIntentSucceededWebhook), typeof(OnPaymentIntentSucceededNotification) },
        { typeof(OnPaymentSuccessWebhook), typeof(OnPaymentSuccessNotification) },
    };

    [Theory]
    [MemberData(nameof(Mappings))]
    public void Webhook_fires_on_its_own_notification(Type webhook, Type notification)
    {
        var baseType = webhook.BaseType!;
        Assert.True(baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(WebhookEventBase<>), $"{webhook.Name} is not a WebhookEventBase<>");
        Assert.Equal(notification, baseType.GetGenericArguments()[0]);
    }

    [Fact]
    public void Every_Stripe_webhook_is_covered()
    {
        var covered = Mappings.Select(row => (Type)row[0]).ToHashSet();
        var webhooks = typeof(OnShippingRateSavedWebhook).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.GetCustomAttributes(typeof(WebhookEventAttribute), false).Length > 0)
            .ToList();

        Assert.NotEmpty(webhooks);
        Assert.All(webhooks, t => Assert.Contains(t, covered));
    }
}
#endif
