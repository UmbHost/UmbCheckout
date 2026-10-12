using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using UmbHost.Licensing.Controllers;
using UmbHost.Licensing.Models.Api;
using Umbraco.Cms.Api.Common.Attributes;
using Xunit;

namespace UmbCheckout.Tests.Api
{
    public class LicensingApiControllerTests
    {
        [Theory]
        [InlineData(typeof(LicenseStatusApiController))]
        [InlineData(typeof(LicenceCheckController))]
        public void LicensingControllers_MapToLicensingDocument(Type controller)
            => Assert.Equal("umbhost-licensing", controller.GetCustomAttribute<MapToApiAttribute>(inherit: true)!.ApiName);

        [Fact]
        public void LicenseStatusResponseModel_ValidityComesFromLicenseCheck_NotStatusText()
        {
            // UmbHost.Licensing decides validity from the domain/path check (UmbCheckoutSettings.IsLicensed); Status is display text.
            var licensed = LicenseStatusResponseModel.From(status: "Active", isDevelopment: false, isLicensed: true, expiry: "2027-01-01", validDomains: "example.com", addons: []);
            var unlicensed = LicenseStatusResponseModel.From(status: "Active", isDevelopment: false, isLicensed: false, expiry: "", validDomains: "", addons: []);

            Assert.True(licensed.IsValid);
            Assert.Equal("Active", licensed.Status);
            Assert.False(unlicensed.IsValid);
        }

        [Fact]
        public void LicenseStatusResponseModel_CarriesTheV13LicenceDetails()
        {
            // The v13 Licence Details panel shows Registration Date and Valid Path alongside the other fields.
            var model = LicenseStatusResponseModel.From(status: "Active", isDevelopment: true, isLicensed: true, expiry: "0000-00-00",
                validDomains: "localhost", addons: [], regDate: "2024-03-01", validPaths: "/");

            Assert.Equal("2024-03-01", model.RegDate);
            Assert.Equal("/", model.ValidPaths);
        }
    }
}
