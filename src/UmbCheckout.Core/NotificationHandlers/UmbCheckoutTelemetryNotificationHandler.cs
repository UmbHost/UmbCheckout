using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using UmbCheckout.Shared;
using UmbCheckout.Shared.Notifications.Configuration;
using UmbHost.Licensing.Models;
using UmbHost.Licensing.Notifications;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Extensions;
using JsonSerializer = System.Text.Json.JsonSerializer;
using UmbCheckoutAppSettings = UmbCheckout.Shared.Models.UmbCheckoutAppSettings;
using Microsoft.Extensions.Hosting;

namespace UmbCheckout.Core.NotificationHandlers
{
    public class UmbCheckoutTelemetryNotificationHandler : INotificationAsyncHandler<OnConfigurationSavedNotification>, INotificationAsyncHandler<OnLicenseCheckCompletedNotification>
    {
        private readonly UmbCheckoutAppSettings _umbCheckoutConfiguration;
        private readonly GlobalSettings _globalSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<UmbCheckoutTelemetryNotificationHandler> _logger;

        /// <summary>The named HttpClient telemetry posts through (5 second timeout).</summary>
        internal const string HttpClientName = "UmbCheckout.Telemetry";

        internal static void ConfigureClient(HttpClient client) => client.Timeout = TimeSpan.FromSeconds(5);

        public UmbCheckoutTelemetryNotificationHandler(IOptions<UmbCheckoutAppSettings> umbCheckoutConfiguration, IOptions<GlobalSettings> globalSettings, IHttpClientFactory httpClientFactory, IUmbracoVersion umbracoVersion, IWebHostEnvironment webHostEnvironment,
            ILogger<UmbCheckoutTelemetryNotificationHandler> logger)
        {
            _logger = logger;
            _umbCheckoutConfiguration = umbCheckoutConfiguration.Value;
            _globalSettings = globalSettings.Value;
            _httpClientFactory = httpClientFactory;
            _umbracoVersion = umbracoVersion;
            _webHostEnvironment = webHostEnvironment;
        }

        [Obsolete("Use the constructor that takes an ILogger. Scheduled for removal in 17.1.")]
        public UmbCheckoutTelemetryNotificationHandler(IOptions<UmbCheckoutAppSettings> umbCheckoutConfiguration, IOptions<GlobalSettings> globalSettings, IHttpClientFactory httpClientFactory, IUmbracoVersion umbracoVersion, IWebHostEnvironment webHostEnvironment)
            : this(umbCheckoutConfiguration, globalSettings, httpClientFactory, umbracoVersion, webHostEnvironment, NullLogger<UmbCheckoutTelemetryNotificationHandler>.Instance)
        {
        }

        // The licence check awaits these handlers: start the post and return, so telemetry never delays licensing or Refresh.
        public Task HandleAsync(OnConfigurationSavedNotification notification, CancellationToken cancellationToken)
        {
            PingTelemetryServer();
            return Task.CompletedTask;
        }

        public Task HandleAsync(OnLicenseCheckCompletedNotification notification, CancellationToken cancellationToken)
        {
            PingTelemetryServer();
            return Task.CompletedTask;
        }

        private void PingTelemetryServer()
        {
            if (_umbCheckoutConfiguration.DisableTelemetry)
            {
                return;
            }

            var umbracoId = Guid.TryParse(_globalSettings.Id, out var telemetrySiteIdentifier)
                ? telemetrySiteIdentifier
                : Guid.Empty;

            if (umbracoId.Equals(Guid.Empty))
            {
                return;
            }

            // Not the notification's token: the post outlives the request that triggered it, bounded by the client's timeout.
            _ = Task.Run(() => PostAsync(umbracoId));
        }

        private async Task PostAsync(Guid umbracoId)
        {
            try
            {
                var data = new
                {
                    umbracoId = umbracoId,
                    umbracoVersion = _umbracoVersion.SemanticVersion.ToSemanticStringWithoutBuild(),
                    umbCheckoutVersion = UmbCheckoutVersion.SemanticVersion.ToString(),
                    isLicensed = UmbCheckoutSettings.IsLicensed.ToString(),
                    isDevelopmentLicense = UmbCheckoutSettings.LicenseDetails.IsDevelopmentLicense.ToString(),
                    environmentName = _webHostEnvironment.EnvironmentName
                };

                var json = JsonConvert.SerializeObject(data, Formatting.None);
                var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
                using var payload = new StringContent(base64, Encoding.UTF8, MediaTypeNames.Text.Plain);

                using var client = _httpClientFactory.CreateClient(HttpClientName);
                using var post = await client.PostAsync(new Uri(Consts.TelemetryUrl), payload);
            }
            catch (Exception ex)
            {
                // Telemetry is best effort: an unreachable or slow server is not the site's problem.
                _logger.LogDebug(ex, "Posting UmbCheckout telemetry failed");
            }
        }
    }
}