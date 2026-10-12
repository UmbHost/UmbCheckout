using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Stripe;
using Xunit;

namespace UmbCheckout.Tests.Infrastructure
{
    /// <summary>
    /// Runs stripe/stripe-mock in a container (same setup as UmbPanel.Stripe.Tests). Requires Docker.
    /// </summary>
    public sealed class StripeMockFixture : IAsyncLifetime
    {
        private const int StripeMockPort = 12111;
        private IContainer _container = default!;

        public string BaseUrl { get; private set; } = default!;

        public IStripeClient CreateClient(HttpMessageHandler? handler = null) => new StripeClient(
            "sk_test_123",
            httpClient: new SystemNetHttpClient(new HttpClient(handler ?? new HttpClientHandler())),
            apiBase: BaseUrl);

        public async Task InitializeAsync()
        {
            _container = new ContainerBuilder("stripe/stripe-mock@sha256:6a3fbbb37639a15525f8df5ae6995a9da21d5e827b897898a47a88e2b6e3790b") // stripe-mock 0.201.0, pinned so tests don't change underneath us
                .WithPortBinding(StripeMockPort, assignRandomHostPort: true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(StripeMockPort))
                .Build();

            await _container.StartAsync();
            BaseUrl = $"http://{_container.Hostname}:{_container.GetMappedPublicPort(StripeMockPort)}";
        }

        public async Task DisposeAsync() => await _container.DisposeAsync();
    }

    [CollectionDefinition("stripe-mock")]
    public sealed class StripeMockCollection : ICollectionFixture<StripeMockFixture>
    {
    }

    /// <summary>Records the URL and form body Stripe.net sends, since stripe-mock answers with canned fixtures.</summary>
    public sealed class CapturingHandler() : DelegatingHandler(new HttpClientHandler())
    {
        public List<string> RequestUris { get; } = [];

        /// <summary>URL-decoded form bodies, e.g. "line_items[0][price_data][unit_amount]=500".</summary>
        public List<string> RequestBodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUris.Add(Uri.UnescapeDataString(request.RequestUri!.ToString()));
            if (request.Content != null)
            {
                RequestBodies.Add(Uri.UnescapeDataString(await request.Content.ReadAsStringAsync(cancellationToken)));
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
