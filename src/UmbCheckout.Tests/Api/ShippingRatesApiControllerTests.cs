using Microsoft.AspNetCore.Mvc;
using Moq;
using UmbCheckout.Stripe.Controllers.BackOffice.Api;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Models;
using UmbCheckout.Stripe.Models.Api;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Xunit;

namespace UmbCheckout.Tests.Api
{
    public class ShippingRatesApiControllerTests
    {
        private readonly Mock<IStripeShippingRateDatabaseService> _db = new();
        private readonly Mock<IStripeShippingRateApiService> _stripe = new();
        private StripeShippingRatesApiController Controller => new(_db.Object, _stripe.Object);

        [Fact]
        public async Task GetAll_PagesWithTotal()
        {
            _db.Setup(s => s.GetShippingRates()).ReturnsAsync(Enumerable.Range(1, 5).Select(i => new ShippingRate { Key = Guid.NewGuid(), Name = $"Rate {i}", Value = $"shr_{i}" }));

            var page = Assert.IsType<PagedViewModel<ShippingRateResponseModel>>(Assert.IsType<OkObjectResult>(await Controller.GetShippingRates(skip: 2, take: 2)).Value);

            Assert.Equal(5, page.Total);
            Assert.Equal(new[] { "Rate 3", "Rate 4" }, page.Items.Select(i => i.Name));
        }

        [Fact]
        public async Task GetById_UnknownId_Returns404ProblemDetails()
        {
            _db.Setup(s => s.GetShippingRate(It.IsAny<Guid>())).ReturnsAsync((ShippingRate?)null);

            var result = Assert.IsAssignableFrom<ObjectResult>(await Controller.GetShippingRateById(Guid.NewGuid()));

            Assert.Equal(404, result.StatusCode);
            Assert.IsType<ProblemDetails>(result.Value);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtId_WithClientSuppliedId()
        {
            var id = Guid.NewGuid();
            _db.Setup(s => s.GetShippingRate("shr_1")).ReturnsAsync((ShippingRate?)null);
            _db.Setup(s => s.CreateShippingRate(It.Is<ShippingRate>(r => r.Key == id && r.Value == "shr_1"))).ReturnsAsync((ShippingRate r) => r);

            var result = await Controller.CreateShippingRate(new CreateShippingRateRequestModel { Id = id, Name = "1 Day", Value = "shr_1" });

            // Umbraco's documented CreatedAtId returns an EmptyCreatedAtActionResult, which writes 201 + Location when executed.
            Assert.IsType<Umbraco.Cms.Api.Common.Mvc.ActionResults.EmptyCreatedAtActionResult>(result);
            _db.Verify(s => s.CreateShippingRate(It.Is<ShippingRate>(r => r.Key == id)), Times.Once);
        }

        [Fact]
        public async Task Create_DuplicateStripeId_Returns400ProblemDetails()
        {
            _db.Setup(s => s.GetShippingRate("shr_1")).ReturnsAsync(new ShippingRate { Value = "shr_1" });

            var result = Assert.IsAssignableFrom<ObjectResult>(await Controller.CreateShippingRate(new CreateShippingRateRequestModel { Name = "Dup", Value = "shr_1" }));

            Assert.Equal(400, result.StatusCode);
            // The back-office notification shows the ProblemDetails title, so it must carry the user-facing message.
            Assert.Equal("A shipping rate with this Stripe ID already exists", Assert.IsType<ProblemDetails>(result.Value).Title);
            _db.Verify(s => s.CreateShippingRate(It.IsAny<ShippingRate>()), Times.Never);
        }

        [Fact]
        public async Task StripeRates_StripeNotConfigured_ReturnsProblemDetails()
        {
            _stripe.Setup(s => s.GetShippingRates()).ThrowsAsync(new UmbCheckout.Stripe.StripeNotConfiguredException(live: false));

            var result = Assert.IsAssignableFrom<ObjectResult>(await Controller.GetStripeShippingRates());

            Assert.Equal(400, result.StatusCode);
            Assert.Equal(UmbCheckout.Core.ManagementApi.UmbCheckoutProblemDetails.StripeNotConfiguredTitle, Assert.IsType<ProblemDetails>(result.Value).Title);
        }

        [Fact]
        public async Task Update_UnknownId_Returns404()
        {
            _db.Setup(s => s.GetShippingRate(It.IsAny<Guid>())).ReturnsAsync((ShippingRate?)null);

            var result = Assert.IsAssignableFrom<ObjectResult>(await Controller.UpdateShippingRate(Guid.NewGuid(), new UpdateShippingRateRequestModel { Name = "x", Value = "shr_x" }));

            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task Delete_UnknownId_Returns404()
        {
            _db.Setup(s => s.DeleteShippingRate(It.IsAny<Guid>())).ReturnsAsync(false);

            var result = Assert.IsAssignableFrom<ObjectResult>(await Controller.DeleteShippingRate(Guid.NewGuid()));

            Assert.Equal(404, result.StatusCode);
        }
    }
}
