using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using UmbCheckout.Backoffice.Controllers.Api;
using UmbCheckout.Core.ManagementApi;
using UmbCheckout.Shared.Notifications;
using UmbCheckout.Stripe.Controllers.BackOffice.Api;
using Xunit;

namespace UmbCheckout.Tests.Api
{
    /// <summary>
    /// Editors manage what is stored: the screens that load data and save it back read without Loaded, so a site's display
    /// changes are neither shown as stored values nor saved back.
    /// </summary>
    public class ManagementReadsTests
    {
        [Theory]
        [InlineData(typeof(ConfigurationApiController))]
        [InlineData(typeof(StripeSettingsApiController))]
        [InlineData(typeof(StripeShippingRatesApiController))]
        public void Editing_screens_read_stored_data(Type controller)
            => Assert.NotNull(controller.GetCustomAttribute<SuppressLoadedHooksAttribute>());

        [Fact]
        public async Task The_filter_suppresses_Loaded_for_the_action_only()
        {
            var context = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
            var executing = new ActionExecutingContext(context, [], new Dictionary<string, object?>(), controller: new object());
            bool? during = null;

            await new SuppressLoadedHooksAttribute().OnActionExecutionAsync(executing, () =>
            {
                during = UmbCheckoutHooks.LoadedSuppressed;
                return Task.FromResult(new ActionExecutedContext(context, [], new object()));
            });

            Assert.True(during);
            Assert.False(UmbCheckoutHooks.LoadedSuppressed);
        }
    }
}
