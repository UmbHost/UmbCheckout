using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Mime;
using System.Text;
using UmbCheckout.Shared;
using UmbCheckout.Shared.Notifications.Configuration;
using UmbHost.Licensing.Models;
using UmbHost.Licensing.Notifications;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using JsonSerializer = System.Text.Json.JsonSerializer;
using UmbCheckoutAppSettings = UmbCheckout.Shared.Models.UmbCheckoutAppSettings;

namespace UmbCheckout.Core.NotificationHandlers
{
    public class UmbCheckoutTelemetryNotificationHandler : INotificationAsyncHandler<OnConfigurationSavedNotification>, INotificationAsyncHandler<OnLicenseCheckCompletedNotification>
    {
        private readonly UmbCheckoutAppSettings _umbCheckoutConfiguration;
        private readonly GlobalSettings _globalSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly IPackagingService _packagingService;

        public UmbCheckoutTelemetryNotificationHandler(IOptions<UmbCheckoutAppSettings> umbCheckoutConfiguration, IOptions<GlobalSettings> globalSettings, IHttpClientFactory httpClientFactory, IUmbracoVersion umbracoVersion, IPackagingService packagingService)
        {

            _umbCheckoutConfiguration = umbCheckoutConfiguration.Value;
            _globalSettings = globalSettings.Value;
            _httpClientFactory = httpClientFactory;
            _umbracoVersion = umbracoVersion;
            _packagingService = packagingService;
        }

        public async Task HandleAsync(OnConfigurationSavedNotification notification, CancellationToken cancellationToken)
        {
            await PingTelemetryServer(cancellationToken);
        }

        public async Task HandleAsync(OnLicenseCheckCompletedNotification notification, CancellationToken cancellationToken)
        {
            await PingTelemetryServer(cancellationToken);
        }

        private async Task PingTelemetryServer(CancellationToken cancellationToken)
        {
            try
            {
                if (_umbCheckoutConfiguration.DisableTelemetry)
                {
                    return;
                }

                var umbracoId = Guid.TryParse(_globalSettings.Id, out var telemetrySiteIdentifier)
                    ? telemetrySiteIdentifier
                    : Guid.Empty;

                if (umbracoId.Equals(Guid.Empty) == true)
                {
                    return;
                }

                var installedPackages = _packagingService.GetAllInstalledPackages()
                    .Where(x => !string.IsNullOrEmpty(x.PackageName) && x.PackageName.StartsWith("UmbCheckout."));

                var data = new
                {
                    umbracoId = umbracoId,
                    umbracoVersion = _umbracoVersion.SemanticVersion.ToSemanticStringWithoutBuild(),
                    umbCheckoutVersion = UmbCheckoutVersion.SemanticVersion.ToString(),
                    installedPackages = JsonSerializer.Serialize(installedPackages),
                    isLicensed = UmbCheckoutSettings.IsLicensed.ToString()
                };

                var json = JsonConvert.SerializeObject(data, Formatting.None);
                var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
                var payload = new StringContent(base64, Encoding.UTF8, MediaTypeNames.Text.Plain);
                var address = new Uri(Consts.TelemetryUrl);

                using var client = _httpClientFactory.CreateClient();
                using var post = await client.PostAsync(address, payload, cancellationToken);
            }
            catch (Exception)
            {
            }
        }
    }
}
