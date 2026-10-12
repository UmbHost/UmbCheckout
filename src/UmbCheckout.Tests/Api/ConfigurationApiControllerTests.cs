using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using UmbCheckout.Backoffice.Controllers.Api;
using UmbCheckout.Backoffice.Models.Api;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Xunit;

namespace UmbCheckout.Tests.Api
{
    public class ConfigurationApiControllerTests
    {
        private static readonly Guid SuccessKey = Guid.Parse("38b08814-0307-4f92-930a-30751217a7c1");

        private static ConfigurationApiController CreateController(Mock<IConfigurationService> service, bool isLicensed = true, string appUrl = "https://localhost:44380/")
        {
            var contentCache = new Mock<IPublishedContentCache>();
            var page = new Mock<IPublishedContent>();
            page.SetupGet(p => p.Key).Returns(SuccessKey);
            page.SetupGet(p => p.Name).Returns("Basket");
            contentCache.Setup(c => c.GetById(SuccessKey)).Returns(page.Object);

            var urlProvider = new Mock<IPublishedUrlProvider>();
            urlProvider.Setup(u => u.GetUrl(page.Object, It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>())).Returns("/basket/");

            return new ConfigurationApiController(
                service.Object,
                contentCache.Object,
                urlProvider.Object,
                Options.Create(new WebRoutingSettings { UmbracoApplicationUrl = appUrl }),
                () => isLicensed);
        }

        [Fact]
        public async Task Get_ReturnsTypedConfiguration_WithPageKeys()
        {
            var service = new Mock<IConfigurationService>();
            service.Setup(s => s.GetConfigurationAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new UmbCheckoutConfiguration
            {
                CurrencyCode = "GBP",
                SuccessPageUrl = [new MultiUrlPicker { Udi = $"umb://document/{SuccessKey:N}", Name = "Basket" }],
                StoreBasketInCookie = true,
                BasketInCookieExpiry = 30
            });

            var result = await CreateController(service).GetConfiguration();

            var ok = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<ConfigurationResponseModel>(ok.Value);
            Assert.Equal("GBP", model.CurrencyCode);
            Assert.Equal(SuccessKey, model.SuccessPageKey);
            Assert.Null(model.CancelPageKey);
            Assert.True(model.StoreBasketInCookie);
            Assert.True(model.IsLicensed);
        }

        [Fact]
        public async Task Put_PassesBasketStorageThrough_TheAddOnAppliesTheLicence()
        {
            var service = new Mock<IConfigurationService>();
            UmbCheckoutConfiguration? saved = null;
            service.Setup(s => s.UpdateConfigurationAsync(It.IsAny<UmbCheckoutConfiguration>(), It.IsAny<CancellationToken>()))
                .Callback<UmbCheckoutConfiguration, CancellationToken>((c, _) => saved = c)
                .ReturnsAsync(true);

            var result = await CreateController(service, isLicensed: false).UpdateConfiguration(new UpdateConfigurationRequestModel
            {
                CurrencyCode = "gbp",
                StoreBasketInCookie = true,
                StoreBasketInDatabase = true,
                SuccessPageKey = SuccessKey
            });

            Assert.IsType<OkResult>(result);
            Assert.NotNull(saved);
            Assert.Equal("GBP", saved!.CurrencyCode);
            Assert.True(saved.StoreBasketInCookie);
            Assert.True(saved.StoreBasketInDatabase);
            var picker = Assert.Single(saved.SuccessPageUrl);
            Assert.Equal($"umb://document/{SuccessKey:N}", picker.Udi);
            Assert.Equal("/basket/", picker.Url);
        }

        [Fact]
        public async Task Put_ServiceFails_Returns400ProblemDetails()
        {
            var service = new Mock<IConfigurationService>();
            service.Setup(s => s.UpdateConfigurationAsync(It.IsAny<UmbCheckoutConfiguration>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await CreateController(service).UpdateConfiguration(new UpdateConfigurationRequestModel { CurrencyCode = "GBP" });

            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
            Assert.IsType<ProblemDetails>(objectResult.Value);
        }

        [Fact]
        public async Task A_cancelled_save_is_a_400_with_the_handlers_message()
        {
            var service = new Mock<IConfigurationService>();
            service.Setup(s => s.UpdateConfigurationAsync(It.IsAny<UmbCheckoutConfiguration>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await CancelledRequest.With(CreateController(service), "Currency changes are frozen")
                .UpdateConfiguration(new UpdateConfigurationRequestModel { CurrencyCode = "GBP" });

            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
            Assert.Equal("Currency changes are frozen", Assert.IsType<ProblemDetails>(objectResult.Value).Title);
        }

        [Fact]
        public async Task Put_UnknownPageKey_Returns400ProblemDetails()
        {
            var service = new Mock<IConfigurationService>();

            var result = await CreateController(service).UpdateConfiguration(new UpdateConfigurationRequestModel
            {
                CurrencyCode = "GBP",
                CancelPageKey = Guid.NewGuid()
            });

            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
            service.Verify(s => s.UpdateConfigurationAsync(It.IsAny<UmbCheckoutConfiguration>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void ApplicationUrl_ReportsWhetherSet()
        {
            var model = Assert.IsType<ApplicationUrlStatusResponseModel>(
                Assert.IsType<OkObjectResult>(CreateController(new Mock<IConfigurationService>(), appUrl: "").GetApplicationUrlStatus()).Value);

            Assert.False(model.IsSet);
        }
    }
}
