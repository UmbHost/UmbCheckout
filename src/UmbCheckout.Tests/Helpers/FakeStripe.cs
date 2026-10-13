using System.Net;
using System.Text;
using Moq;
using Stripe;
using UmbCheckout.Stripe;
using UmbCheckout.Stripe.Interfaces;

namespace UmbCheckout.Tests.Helpers;

/// <summary>A Stripe client that answers every request with a canned JSON body (no network), recording the requests.</summary>
public sealed class FakeStripeHandler : HttpMessageHandler
{
    private readonly string _json;
    private readonly HttpStatusCode _status;

    public FakeStripeHandler(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        _json = json;
        _status = status;
    }

    public List<HttpRequestMessage> Requests { get; } = new();

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        return Task.FromResult(new HttpResponseMessage(_status) { Content = new StringContent(_json, Encoding.UTF8, "application/json") });
    }

    public IStripeClient Client(string apiKey = "sk_test_fake") =>
        new StripeClient(apiKey, httpClient: new SystemNetHttpClient(new HttpClient(this), maxNetworkRetries: 0));

    public IUmbCheckoutStripeClientFactory Factory(string apiKey = "sk_test_fake")
    {
        var factory = new Mock<IUmbCheckoutStripeClientFactory>();
        factory.Setup(f => f.CreateAsync()).ReturnsAsync(() => Client(apiKey));
        factory.Setup(f => f.CreateAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => Client(apiKey));
        return factory.Object;
    }

    public static IUmbCheckoutStripeClientFactory NotConfigured(bool live = false)
    {
        var factory = new Mock<IUmbCheckoutStripeClientFactory>();
        factory.Setup(f => f.CreateAsync()).ThrowsAsync(new StripeNotConfiguredException(live));
        factory.Setup(f => f.CreateAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new StripeNotConfiguredException(live));
        return factory.Object;
    }
}
