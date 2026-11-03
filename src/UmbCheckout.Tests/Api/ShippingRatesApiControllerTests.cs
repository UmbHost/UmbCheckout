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
            _db.Setup(s => s.GetShippingRatesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Enumerable.Range(1, 5).Select(i => new ShippingRate { Key = Guid.NewGuid(), Name = $"Rate {i}", Value = $"shr_{i}" }));

            var page = Assert.IsType<PagedViewModel<ShippingRateResponseModel>>(Assert.IsType<OkObjectResult>(await Controller.GetShippingRates(skip: 2, take: 2)).Value);

            Assert.Equal(5, page.Total);
            Assert.Equal(new[] { "Rate 3", "Rate 4" }, page.Items.Select(i => i.Name));
        }

        [Fact]
        public async Task GetById_UnknownId_Returns404ProblemDetails()
        {
            _db.Setup(s => s.GetShippingRateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ShippingRate?)null);

            var result = Assert.IsAssignableFrom<ObjectResult>(await Controller.GetShippingRateById(Guid.NewGuid()));

            Assert.Equal(404, result.StatusCode);
            Assert.IsType<ProblemDetails>(result.Value);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtId_WithClientSuppliedId()
        {
            var id = Guid.NewGuid();
            _db.Setup(s => s.GetShippingRateAsync("shr_1", It.IsAny<CancellationToken>())).ReturnsAsync((ShippingRate?)null);
            _db.Setup(s => s.CreateShippingRateAsync(It.Is<ShippingRate>(r => r.Key == id && r.Value == "shr_1"), It.IsAny<CancellationToken>())).ReturnsAsync((ShippingRate r, CancellationToken _) => r);

            var result = await Controller.CreateShippingRate(new CreateShippingRateRequestModel { Id = id, Name = "1 Day", Value = "shr_1" });

            // Umbraco's documented CreatedAtId returns an EmptyCreatedAtActionResult, which writes 201 + Location when executed.
            Assert.IsType<Umbraco.Cms.Api.Common.Mvc.ActionResults.EmptyCreatedAtActionResult>(result);
            _db.Verify(s => s.CreateShippingRateAsync(It.Is<ShippingRate>(r => r.Key == id), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Create_DuplicateStripeId_Returns400ProblemDetails()
        {
            _db.Setup(s => s.GetShippingRateAsync("shr_1", It.IsAny<CancellationToken>())).ReturnsAsync(new ShippingRate { Value = "shr_1" });

            var result = Assert.IsAssignableFrom<ObjectResult>(await Controller.CreateShippingRate(new CreateShippingRateRequestModel { Name = "Dup", Value = "shr_1" }));

            Assert.Equal(400, result.StatusCode);
            // The back-office notification shows the ProblemDetails title, so it must carry the user-facing message.
            Assert.Equal("A shipping rate with this Stripe ID already exists", Assert.IsType<ProblemDetails>(result.Value).Title);
            _db.Verify(s => s.CreateShippingRateAsync(It.IsAny<ShippingRate>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData(System.Net.HttpStatusCode.Unauthorized)]
        [InlineData(System.Net.HttpStatusCode.Forbidden)]
        public async Task StripeRates_RejectedKey_ShowsAFixedDetailWithoutTheKeyTail(System.Net.HttpStatusCode status)
        {
            _stripe.Setup(s => s.GetShippingRates(It.IsAny<CancellationToken>())).ThrowsAsync(new global::Stripe.StripeException("Invalid API Key provided: sk_test_****wxyz")
            {
                HttpStatusCode = status,
                StripeError = new global::Stripe.StripeError { Message = "Invalid API Key provided: sk_test_****wxyz" }
            });

            var result = Assert.IsAssignableFrom<ObjectResult>(await Controller.GetStripeShippingRates());

            var detail = Assert.IsType<ProblemDetails>(result.Value).Detail;
            Assert.Equal(UmbCheckout.Stripe.Helpers.StripeKeyRejection.Detail, detail);
            Assert.DoesNotContain("wxyz", detail);
        }

        [Fact]
        public async Task StripeRates_StripeNotConfigured_ReturnsProblemDetails()
        {
            _stripe.Setup(s => s.GetShippingRates(It.IsAny<CancellationToken>())).ThrowsAsync(new UmbCheckout.Stripe.StripeNotConfiguredException(live: false));

            var result = Assert.IsAssignableFrom<ObjectResult>(await Controller.GetStripeShippingRates());

            Assert.Equal(400, result.StatusCode);
            Assert.Equal(UmbCheckout.Core.ManagementApi.UmbCheckoutProblemDetails.StripeNotConfiguredTitle, Assert.IsType<ProblemDetails>(result.Value).Title);
        }

        [Fact]
        public async Task Update_UnknownId_Returns404()
        {
            _db.Setup(s => s.GetShippingRateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ShippingRate?)null);

            var result = Assert.IsAssignableFrom<ObjectResult>(await Controller.UpdateShippingRate(Guid.NewGuid(), new UpdateShippingRateRequestModel { Name = "x", Value = "shr_x" }));

            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task Delete_UnknownId_Returns404()
        {
            _db.Setup(s => s.DeleteShippingRateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = Assert.IsAssignableFrom<ObjectResult>(await Controller.DeleteShippingRate(Guid.NewGuid()));

            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task A_cancelled_create_is_a_400_with_the_handlers_message()
        {
            _db.Setup(s => s.GetShippingRateAsync("shr_1", It.IsAny<CancellationToken>())).ReturnsAsync((ShippingRate?)null);
            _db.Setup(s => s.CreateShippingRateAsync(It.IsAny<ShippingRate>(), It.IsAny<CancellationToken>())).ReturnsAsync((ShippingRate?)null);

            var result = Assert.IsAssignableFrom<ObjectResult>(await CancelledRequest.With(Controller, "Rates are locked")
                .CreateShippingRate(new CreateShippingRateRequestModel { Name = "A", Value = "shr_1" }));

            Assert.Equal(400, result.StatusCode);
            var problem = Assert.IsType<ProblemDetails>(result.Value);
            // A short title and the handler's message once (the toast shows both).
            Assert.Equal(UmbCheckout.Core.ManagementApi.UmbCheckoutProblemDetails.SaveCancelledTitle, problem.Title);
            Assert.Equal("Rates are locked", problem.Detail);
        }

        [Fact]
        public async Task A_cancelled_update_is_a_400_with_the_handlers_message()
        {
            var key = Guid.NewGuid();
            _db.Setup(s => s.GetShippingRateAsync(key, It.IsAny<CancellationToken>())).ReturnsAsync(new ShippingRate { Key = key, Name = "A", Value = "shr_1" });
            _db.Setup(s => s.UpdateShippingRateAsync(It.IsAny<ShippingRate>(), It.IsAny<CancellationToken>())).ReturnsAsync((ShippingRate?)null);

            var result = Assert.IsAssignableFrom<ObjectResult>(await CancelledRequest.With(Controller, "Rates are locked")
                .UpdateShippingRate(key, new UpdateShippingRateRequestModel { Name = "B", Value = "shr_1" }));

            Assert.Equal(400, result.StatusCode);
            var problem = Assert.IsType<ProblemDetails>(result.Value);
            // A short title and the handler's message once (the toast shows both).
            Assert.Equal(UmbCheckout.Core.ManagementApi.UmbCheckoutProblemDetails.SaveCancelledTitle, problem.Title);
            Assert.Equal("Rates are locked", problem.Detail);
        }

        [Fact]
        public async Task A_cancelled_delete_is_a_400_not_a_404()
        {
            _db.Setup(s => s.DeleteShippingRateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = Assert.IsAssignableFrom<ObjectResult>(await CancelledRequest.With(Controller, "Rates are locked").DeleteShippingRate(Guid.NewGuid()));

            Assert.Equal(400, result.StatusCode);
            var problem = Assert.IsType<ProblemDetails>(result.Value);
            // A short title and the handler's message once (the toast shows both).
            Assert.Equal(UmbCheckout.Core.ManagementApi.UmbCheckoutProblemDetails.DeleteCancelledTitle, problem.Title);
            Assert.Equal("Rates are locked", problem.Detail);
        }

        [Fact]
        public async Task A_duplicate_without_a_cancel_keeps_todays_response()
        {
            _db.Setup(s => s.GetShippingRateAsync("shr_1", It.IsAny<CancellationToken>())).ReturnsAsync(new ShippingRate { Key = Guid.NewGuid(), Value = "shr_1" });

            var result = Assert.IsAssignableFrom<ObjectResult>(await CancelledRequest.With(Controller, null)
                .CreateShippingRate(new CreateShippingRateRequestModel { Name = "A", Value = "shr_1" }));

            Assert.Equal(400, result.StatusCode);
            Assert.Equal("A shipping rate with this Stripe ID already exists", Assert.IsType<ProblemDetails>(result.Value).Title);
        }

        [Fact]
        public async Task A_cancel_already_reported_does_not_turn_a_later_404_into_a_400()
        {
            // Review minor: the request's cancel marker is cleared once it has been reported.
            _db.Setup(s => s.DeleteShippingRateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var controller = CancelledRequest.With(Controller, "Rates are locked");

            Assert.Equal(400, Assert.IsAssignableFrom<ObjectResult>(await controller.DeleteShippingRate(Guid.NewGuid())).StatusCode);
            Assert.Equal(404, Assert.IsAssignableFrom<ObjectResult>(await controller.DeleteShippingRate(Guid.NewGuid())).StatusCode);
        }

        [Fact]
        public async Task A_failed_delete_without_a_cancel_is_still_a_404()
        {
            _db.Setup(s => s.DeleteShippingRateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = Assert.IsAssignableFrom<ObjectResult>(await CancelledRequest.With(Controller, null).DeleteShippingRate(Guid.NewGuid()));

            Assert.Equal(404, result.StatusCode);
        }
    }
}
