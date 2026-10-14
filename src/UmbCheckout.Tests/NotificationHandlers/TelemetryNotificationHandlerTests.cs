using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UmbCheckout.Core.NotificationHandlers;
using UmbCheckout.Shared.Models;
using UmbCheckout.Shared.Notifications.Configuration;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Services;
using Xunit;

namespace UmbCheckout.Tests.NotificationHandlers;

/// <summary>
/// Quality Phase 6: telemetry must never delay the backoffice Refresh button. Publishers await this handler,
/// so it starts the post and returns; a slow or failing telemetry server is logged at Debug and nothing else.
/// </summary>
public sealed class TelemetryNotificationHandlerTests
{
    private readonly RecordingLogger _logger = new();
    private readonly Mock<IHttpClientFactory> _clients = new();

    private UmbCheckoutTelemetryNotificationHandler Handler(HttpMessageHandler server)
    {
        _clients.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(server, disposeHandler: false));
        var version = new Mock<IUmbracoVersion>();
        version.SetupGet(v => v.SemanticVersion).Returns(new SemVersion(17, 6, 0));
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(e => e.EnvironmentName).Returns("Development");
        var packaging = new Mock<IPackagingService>();
        packaging.Setup(p => p.GetAllInstalledPackagesAsync()).ReturnsAsync(new[]
        {
            new InstalledPackage { PackageName = "UmbCheckout.Stripe" },
            new InstalledPackage { PackageName = "Some.Other.Package" },
        });

        return new UmbCheckoutTelemetryNotificationHandler(Options.Create(new UmbCheckoutAppSettings()), Options.Create(new GlobalSettings { Id = Guid.NewGuid().ToString() }),
            _clients.Object, version.Object, environment.Object, packaging.Object, _logger);
    }

    [Fact]
    public async Task Returns_before_a_server_that_never_answers()
    {
        var handler = Handler(new Server((_, ct) => Task.Delay(Timeout.Infinite, ct).ContinueWith(_ => new HttpResponseMessage(), TaskScheduler.Default)));
        var stopwatch = Stopwatch.StartNew();

        var handled = handler.HandleAsync(new OnConfigurationSavedNotification(new UmbCheckoutConfiguration()), CancellationToken.None);
        var first = await Task.WhenAny(handled, Task.Delay(TimeSpan.FromSeconds(5)));

        Assert.Same(handled, first);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(1), $"took {stopwatch.Elapsed}");
    }

    [Fact]
    public async Task A_failed_post_is_logged_at_Debug_only()
    {
        var handler = Handler(new Server((_, _) => throw new HttpRequestException("telemetry host down")));

        await handler.HandleAsync(new OnConfigurationSavedNotification(new UmbCheckoutConfiguration()), CancellationToken.None);
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (_logger.Entries.IsEmpty && DateTime.UtcNow < deadline)
        {
            await Task.Delay(20);
        }

        var entry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Debug, entry);
    }

    [Fact]
    public async Task Posts_through_its_own_client_with_a_5_second_timeout()
    {
        var posted = new TaskCompletionSource();
        var handler = Handler(new Server((_, _) =>
        {
            posted.TrySetResult();
            return Task.FromResult(new HttpResponseMessage());
        }));

        await handler.HandleAsync(new OnConfigurationSavedNotification(new UmbCheckoutConfiguration()), CancellationToken.None);
        await posted.Task.WaitAsync(TimeSpan.FromSeconds(5));

        _clients.Verify(f => f.CreateClient(UmbCheckoutTelemetryNotificationHandler.HttpClientName), Times.Once);
        using var client = new HttpClient();
        UmbCheckoutTelemetryNotificationHandler.ConfigureClient(client);
        Assert.Equal(TimeSpan.FromSeconds(5), client.Timeout);
    }

    [Fact]
    public async Task Reports_the_installed_UmbCheckout_packages()
    {
        var body = new TaskCompletionSource<string>();
        var handler = Handler(new Server(async (request, _) =>
        {
            var base64 = await request.Content.Required().ReadAsStringAsync();
            body.TrySetResult(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64)));
            return new HttpResponseMessage();
        }));

        await handler.HandleAsync(new OnConfigurationSavedNotification(new UmbCheckoutConfiguration()), CancellationToken.None);
        var json = await body.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Contains("installedPackages", json);
        Assert.Contains("UmbCheckout.Stripe", json);
        Assert.DoesNotContain("Some.Other.Package", json);
    }

    private sealed class Server(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => send(request, cancellationToken);
    }

    private sealed class RecordingLogger : ILogger<UmbCheckoutTelemetryNotificationHandler>
    {
        public ConcurrentQueue<LogLevel> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Entries.Enqueue(logLevel);
    }
}
