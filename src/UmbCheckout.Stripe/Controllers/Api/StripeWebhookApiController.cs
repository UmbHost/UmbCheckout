using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using UmbCheckout.Stripe.Models;
using UmbCheckout.Stripe.Notifications.Webhooks;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Controllers;

namespace UmbCheckout.Stripe.Controllers.Api
{
    [PluginController("UmbCheckout")]
    public class StripeWebhookApiController : UmbracoApiController
    {
        private readonly ILogger<StripeWebhookApiController> _logger;
        private readonly ICoreScopeProvider _coreScopeProvider;
        private readonly StripeSettings _settings;

        public StripeWebhookApiController(ILogger<StripeWebhookApiController> logger, ICoreScopeProvider coreScopeProvider, IOptionsMonitor<StripeSettings> stripeSettings)
        {
            _logger = logger;
            _coreScopeProvider = coreScopeProvider;
            _settings = stripeSettings.CurrentValue;
        }

        [HttpPost]
        public async Task<IActionResult> CheckoutEvents([FromBody] Stream request)
        {
            var json = await new StreamReader(request).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], _settings.WebHookSecret);

                switch (stripeEvent.Type)
                {
                    case Events.CheckoutSessionAsyncPaymentFailed:
                    {
                        using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                        scope.Notifications.Publish(new OnPaymentFailedNotification(stripeEvent));
                        break;
                    }
                    case Events.CheckoutSessionAsyncPaymentSucceeded:
                    {
                        using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                        scope.Notifications.Publish(new OnPaymentSuccessNotification(stripeEvent));
                        break;
                    }
                    case Events.CheckoutSessionCompleted:
                    {
                        using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                        scope.Notifications.Publish(new OnCheckoutSessionCompletedNotification(stripeEvent));
                        break;
                    }
                    case Events.CheckoutSessionExpired:
                    {
                        using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                        scope.Notifications.Publish(new OnCheckoutSessionExpiredNotification(stripeEvent));
                        break;
                    }
                    default:
                        _logger.LogWarning("Unhandled event type: {0}", stripeEvent.Type);
                        break;
                }

                return Accepted();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, ex.Message);
                return BadRequest();
            }
        }
    }
}
