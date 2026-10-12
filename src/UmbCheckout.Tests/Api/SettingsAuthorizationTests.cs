using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Web.Common.Authorization;
using Xunit;

namespace UmbCheckout.Tests.Api
{
    /// <summary>
    /// These screens live under Settings > UmbCheckout, so changing checkout configuration, Stripe mode/settings and
    /// shipping rates requires Settings section access — not just any back-office login.
    /// </summary>
    public class SettingsAuthorizationTests
    {
        [Theory]
        [InlineData(typeof(UmbCheckout.Backoffice.Controllers.Api.ConfigurationApiController))]
        [InlineData(typeof(UmbCheckout.Stripe.Controllers.BackOffice.Api.StripeSettingsApiController))]
        [InlineData(typeof(UmbCheckout.Stripe.Controllers.BackOffice.Api.StripeShippingRatesApiController))]
        public void SettingsControllers_RequireSettingsSectionAccess(Type controller)
            => Assert.Contains(controller.GetCustomAttributes<AuthorizeAttribute>(inherit: true), a => a.Policy == AuthorizationPolicies.SectionAccessSettings);
    }
}
