using Microsoft.AspNetCore.Authorization;
using UmbCheckout.Backoffice.Controllers.Api;
using UmbCheckout.Stripe.Controllers.BackOffice.Api;
using Umbraco.Cms.Web.Common.Authorization;
using Xunit;

namespace UmbCheckout.Tests.Controllers;

/// <summary>
/// The settings APIs (configuration, Stripe settings, shipping rates) are for Settings section users, as v17: the screens live in
/// the Settings tree, and a backoffice user without it gets a 403.
/// </summary>
public sealed class SettingsAuthorizationTests
{
    [Theory]
    [InlineData(typeof(ConfigurationApiController))]
    [InlineData(typeof(StripeSettingsApiController))]
    [InlineData(typeof(StripeShippingRatesApiController))]
    public void Settings_apis_need_the_Settings_section(Type controller)
    {
        var policies = controller.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .Select(a => a.Policy)
            .ToList();

        Assert.Contains(AuthorizationPolicies.SectionAccessSettings, policies);
    }
}
