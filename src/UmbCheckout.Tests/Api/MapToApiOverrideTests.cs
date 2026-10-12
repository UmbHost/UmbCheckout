using System.Reflection;
using UmbCheckout.Backoffice.Controllers.Api;
using Umbraco.Cms.Api.Common.Attributes;
using Xunit;

namespace UmbCheckout.Tests.Api
{
    public class MapToApiOverrideTests
    {
        [Fact]
        public void ConfigurationController_MapsToUmbCheckoutDocument_NotManagement()
        {
            // Swashbuckle/Umbraco resolve the nearest MapToApi attribute; the subclass must win.
            var attribute = typeof(ConfigurationApiController).GetCustomAttribute<MapToApiAttribute>(inherit: true);

            Assert.NotNull(attribute);
            Assert.Equal("umbcheckout", attribute!.ApiName);
        }
    }
}
