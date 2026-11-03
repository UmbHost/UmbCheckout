using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.NotificationHandlers;
using Umbraco.Cms.Core.Routing;

namespace UmbCheckout.Tests.Services
{
    /// <summary>Builds the success handler for tests (the page is always the success page; the routed page's culture is given).</summary>
    internal static class SuccessHandler
    {
        public static StripeResponseNotificationHandler Create(IStripeSessionService stripe, ISessionService sessions, Func<IPublishedRequestBuilder, string?> culture)
        {
            var matcher = new Moq.Mock<IUmbCheckoutSuccessPageMatcher>();
            matcher.Setup(m => m.IsSuccessPageAsync(Moq.It.IsAny<Umbraco.Cms.Core.Models.PublishedContent.IPublishedContent?>(), Moq.It.IsAny<Uri>(), Moq.It.IsAny<CancellationToken>())).ReturnsAsync(true);
            return new(stripe, sessions, matcher.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger<StripeResponseNotificationHandler>.Instance, culture);
        }
    }
}
