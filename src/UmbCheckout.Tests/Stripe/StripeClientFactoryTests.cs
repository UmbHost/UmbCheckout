#pragma warning disable CS0618 // these tests cover the overloads without a token, which are obsolete but kept
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Stripe;
using UmbCheckout.Stripe;
using UmbCheckout.Stripe.Controllers.BackOffice.Api;
using UmbCheckout.Stripe.Helpers;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Models;
using UmbCheckout.Stripe.Services;
using UmbCheckout.Tests.Helpers;
using Umbraco.Cms.Core.Services;
using Xunit;

namespace UmbCheckout.Tests.Stripe;

/// <summary>
/// Every backoffice Stripe service gets its client from one factory (as v17). A missing or empty key, a Stripe error, or Stripe
/// being unreachable gives a clear problem response the screens can show, instead of a 500.
/// </summary>
public sealed class StripeClientFactoryTests
{
    private static UmbCheckoutStripeClientFactory Factory(string testKey, string liveKey, bool? live)
    {
        var options = new Mock<IOptionsMonitor<StripeSettings>>();
        options.Setup(o => o.CurrentValue).Returns(new StripeSettings
        {
            Test = new StripeApiDetails { ApiKey = testKey },
            Live = new StripeApiDetails { ApiKey = liveKey }
        });
        var settings = new Mock<IStripeSettingsService>();
        settings.Setup(s => s.GetStripeSettings()).ReturnsAsync(live.HasValue ? new UmbCheckoutStripeSettings { UseLiveApiDetails = live.Value } : null);
        return new UmbCheckoutStripeClientFactory(options.Object, settings.Object);
    }

    [Theory]
    [InlineData(false, "sk_test_1")]
    [InlineData(true, "sk_live_1")]
    [InlineData(null, "sk_test_1")]
    public async Task The_saved_mode_picks_the_key(bool? live, string expected)
    {
        var client = await Factory("sk_test_1", "sk_live_1", live).CreateAsync();

        Assert.Equal(expected, client.ApiKey);
    }

    [Theory]
    [InlineData(false, "")]
    [InlineData(false, "  ")]
    [InlineData(true, "")]
    public async Task A_missing_key_is_reported_as_not_configured(bool live, string key)
    {
        var factory = Factory(live ? "sk_test_1" : key, live ? key : "sk_live_1", live);

        var ex = await Record.ExceptionAsync(() => factory.CreateAsync());

        var notConfigured = Assert.IsType<StripeNotConfiguredException>(ex);
        Assert.Equal(live, notConfigured.Live);
        Assert.DoesNotContain("sk_", notConfigured.Message);
    }

    [Fact]
    public async Task The_shipping_rate_service_uses_the_factory_client()
    {
        var handler = new FakeStripeHandler("{\"object\":\"list\",\"data\":[],\"has_more\":false,\"url\":\"/v1/shipping_rates\"}");
        var service = new StripeShippingRateApiService(handler.Factory("sk_test_factory"));

        await service.GetShippingRates();

        var request = Assert.Single(handler.Requests);
        Assert.Equal("Bearer sk_test_factory", request.Headers.Authorization.Required().ToString());
    }

    [Fact]
    public async Task The_shipping_rate_service_reports_a_missing_key()
    {
        var service = new StripeShippingRateApiService(FakeStripeHandler.NotConfigured());

        await Assert.ThrowsAsync<StripeNotConfiguredException>(() => service.GetShippingRates());
        await Assert.ThrowsAsync<StripeNotConfiguredException>(() => service.GetShippingRate("shr_1"));
    }

    public static TheoryData<Exception, int, string> Failures => new()
    {
        { new StripeNotConfiguredException(live: false), 400, "No Stripe API key is configured for test mode. Set UmbCheckout:Stripe:Test:ApiKey." },
        { new StripeException(HttpStatusCode.BadRequest, new StripeError { Message = "No such shipping rate: 'shr_x'" }, "No such shipping rate: 'shr_x'"), 400, "No such shipping rate: 'shr_x'" },
        { new StripeException(HttpStatusCode.NotFound, new StripeError { Message = "No such shipping rate: 'shr_x'" }, "No such shipping rate: 'shr_x'"), 404, "No such shipping rate: 'shr_x'" },
        { new HttpRequestException("Name or service not known"), 502, StripeApiErrorResult.UnreachableDetail },
        { new TaskCanceledException("timeout"), 502, StripeApiErrorResult.UnreachableDetail },
    };

    [Theory]
    [MemberData(nameof(Failures))]
    public void Stripe_failures_become_problem_responses(Exception failure, int status, string detail)
    {
        var result = StripeApiErrorResult.For(failure, NullLogger.Instance);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(status, json.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(json.Value);
        Assert.Equal(status, problem.Status);
        Assert.Equal(detail, problem.Detail);
        Assert.False(string.IsNullOrWhiteSpace(problem.Title));
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    public void A_rejected_key_gives_a_constant_detail_and_log_without_the_key(HttpStatusCode status)
    {
        // Stripe's message for a rejected key echoes the key's last 4 characters
        const string stripeMessage = "Invalid API Key provided: sk_test_********************WXYZ";
        var failure = new StripeException(status, new StripeError { Message = stripeMessage }, stripeMessage);
        var logger = new CapturingLogger();

        var json = Assert.IsType<JsonResult>(StripeApiErrorResult.For(failure, logger));

        Assert.Equal(400, json.StatusCode);
        Assert.Equal(StripeApiErrorResult.KeyRejectedDetail, Assert.IsType<ProblemDetails>(json.Value).Detail);
        Assert.Equal("Stripe rejected the API key for the active mode (test or live). Check the Stripe settings.", StripeApiErrorResult.KeyRejectedDetail);
        var entry = Assert.Single(logger.Entries);
        Assert.Null(entry.Exception);
        Assert.DoesNotContain("WXYZ", entry.Message);
    }

    /// <summary>Records each entry's message and exception (a logged exception carries Stripe's message).</summary>
    private sealed class CapturingLogger : Microsoft.Extensions.Logging.ILogger
    {
        public List<(string Message, Exception? Exception)> Entries { get; } = new();

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullDisposable.Instance;

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Entries.Add((formatter(state, exception), exception));

        private sealed class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new();

            public void Dispose()
            {
            }
        }
    }

    [Fact]
    public void Other_failures_and_aborted_requests_are_not_mapped()
    {
        Assert.Null(StripeApiErrorResult.For(new InvalidOperationException("bug"), NullLogger.Instance));
        Assert.Null(StripeApiErrorResult.For(new TaskCanceledException(), NullLogger.Instance, requestAborted: true));
    }

    [Fact]
    public void Problem_responses_use_the_camel_case_names_the_screens_read()
    {
        var body = JsonResultAssert.Body(StripeApiErrorResult.For(new StripeNotConfiguredException(live: true), NullLogger.Instance).Required());

        Assert.Equal(400, body["status"].Required().GetValue<int>());
        Assert.NotNull(body["title"]);
        Assert.NotNull(body["detail"]);
    }

    [Fact]
    public async Task The_shipping_rate_api_answers_Stripe_failures_with_a_problem_not_a_500()
    {
        var api = new Mock<IStripeShippingRateApiService>();
        api.Setup(a => a.GetShippingRates(It.IsAny<CancellationToken>())).ThrowsAsync(new StripeNotConfiguredException(live: false));
        api.Setup(a => a.GetShippingRate("shr_1", It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("unreachable"));
        var controller = new StripeShippingRatesApiController(NullLogger<StripeShippingRatesApiController>.Instance,
            Mock.Of<IStripeShippingRateDatabaseService>(), api.Object, Mock.Of<ILocalizedTextService>());

        IActionResult? list = null, single = null;
        Assert.Null(await Record.ExceptionAsync(async () => list = await controller.GetStripeShippingRates()));
        Assert.Null(await Record.ExceptionAsync(async () => single = await controller.GetStripeShippingRate("shr_1")));

        Assert.Equal(400, Assert.IsType<JsonResult>(list).StatusCode);
        Assert.Equal(502, Assert.IsType<JsonResult>(single).StatusCode);
    }
}
