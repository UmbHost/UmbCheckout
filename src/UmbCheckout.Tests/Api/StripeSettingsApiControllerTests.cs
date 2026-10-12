using Microsoft.AspNetCore.Mvc;
using Moq;
using UmbCheckout.Stripe.Controllers.BackOffice.Api;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Models;
using UmbCheckout.Stripe.Models.Api;
using Xunit;

namespace UmbCheckout.Tests.Api
{
    public class StripeSettingsApiControllerTests
    {
        [Fact]
        public async Task Get_MapsSettings()
        {
            var service = new Mock<IStripeSettingsService>();
            service.Setup(s => s.GetStripeSettingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new UmbCheckoutStripeSettings { CollectPhoneNumber = true, ShippingAllowedCountries = "GB,US" });

            var model = Assert.IsType<StripeSettingsResponseModel>(Assert.IsType<OkObjectResult>(await new StripeSettingsApiController(service.Object).GetStripeSettings()).Value);

            Assert.True(model.CollectPhoneNumber);
            Assert.Equal("GB,US", model.ShippingAllowedCountries);
        }

        [Fact]
        public async Task Get_NoSettingsYet_ReturnsDefaults()
        {
            var service = new Mock<IStripeSettingsService>();
            service.Setup(s => s.GetStripeSettingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync((UmbCheckoutStripeSettings?)null);

            var model = Assert.IsType<StripeSettingsResponseModel>(Assert.IsType<OkObjectResult>(await new StripeSettingsApiController(service.Object).GetStripeSettings()).Value);

            Assert.False(model.UseLiveApiDetails);
        }

        [Fact]
        public async Task Put_SaveFails_Returns400ProblemDetails()
        {
            var service = new Mock<IStripeSettingsService>();
            service.Setup(s => s.UpdateStripeSettingsAsync(It.IsAny<UmbCheckoutStripeSettings>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = Assert.IsAssignableFrom<ObjectResult>(await new StripeSettingsApiController(service.Object).UpdateStripeSettings(new UpdateStripeSettingsRequestModel()));

            Assert.Equal(400, result.StatusCode);
            Assert.IsType<ProblemDetails>(result.Value);
        }
    }
}
