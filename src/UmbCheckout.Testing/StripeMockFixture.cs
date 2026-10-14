using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Stripe;
using Xunit;

namespace UmbCheckout.Testing;

/// <summary>
/// Runs stripe/stripe-mock in a container (same setup as UmbPanel.Stripe.Tests). Requires Docker.
/// Each test assembly declares its own <c>[CollectionDefinition("stripe-mock")]</c> (xUnit only finds collection definitions in
/// the test assembly), so one container serves the whole assembly.
/// </summary>
public sealed class StripeMockFixture : IAsyncLifetime
{
    /// <summary>stripe-mock 0.201.0, pinned by digest so tests don't change underneath us.</summary>
    public const string Image = "stripe/stripe-mock@sha256:6a3fbbb37639a15525f8df5ae6995a9da21d5e827b897898a47a88e2b6e3790b";

    private const int StripeMockPort = 12111;
    private IContainer? _container;
    private string? _baseUrl;

    public string BaseUrl => _baseUrl ?? throw new InvalidOperationException("The stripe-mock container has not started: the fixture is initialised by xUnit before the tests run.");

    public IStripeClient CreateClient(HttpMessageHandler? handler = null) => new StripeClient(
        "sk_test_123",
        httpClient: new SystemNetHttpClient(new HttpClient(handler ?? new HttpClientHandler())),
        apiBase: BaseUrl);

    public async Task InitializeAsync()
    {
        _container = new ContainerBuilder(Image)
            .WithPortBinding(StripeMockPort, assignRandomHostPort: true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(StripeMockPort))
            .Build();

        await _container.StartAsync();
        _baseUrl = $"http://{_container.Hostname}:{_container.GetMappedPublicPort(StripeMockPort)}";
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}

/// <summary>Records the URL, headers and form body Stripe.net sends, since stripe-mock answers with canned fixtures.</summary>
public sealed class CapturingHandler : DelegatingHandler
{
    public CapturingHandler() : base(new HttpClientHandler())
    {
    }

    public List<string> RequestUris { get; } = new();

    /// <summary>URL-decoded form bodies, e.g. "line_items[0][price_data][unit_amount]=500".</summary>
    public List<string> RequestBodies { get; } = new();

    /// <summary>Each request's headers, e.g. Idempotency-Key.</summary>
    public List<IReadOnlyDictionary<string, string>> RequestHeaders { get; } = new();

    /// <remarks>A request whose token is already cancelled never goes on the wire, so it isn't recorded (it throws, as HttpClient would).</remarks>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RequestUris.Add(Uri.UnescapeDataString(request.RequestUri?.ToString() ?? string.Empty));
        RequestHeaders.Add(request.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value), StringComparer.OrdinalIgnoreCase));
        if (request.Content != null)
        {
            RequestBodies.Add(Uri.UnescapeDataString(await request.Content.ReadAsStringAsync(cancellationToken)));
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
