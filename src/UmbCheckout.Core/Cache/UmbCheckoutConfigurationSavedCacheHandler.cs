using UmbCheckout.Shared.Notifications.Configuration;
using UmbCheckout.Shared.Persistence;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using UmbCheckoutConfiguration = UmbCheckout.Core.Pocos.UmbCheckoutConfiguration;

namespace UmbCheckout.Core.Cache
{
    /// <summary>
    /// Clears the cached configuration once a save has committed. Umbraco publishes Saved when the outermost scope completes, so this
    /// also covers a save inside someone else's scope (a uSync import), where the service's own clear runs before the commit.
    /// </summary>
    internal sealed class UmbCheckoutConfigurationSavedCacheHandler(SettingsCache<UmbCheckoutConfiguration> cache, DistributedCache? distributedCache = null)
        : INotificationHandler<OnConfigurationSavedNotification>
    {
        public void Handle(OnConfigurationSavedNotification notification)
        {
            // This server's copy now, every other server's through Umbraco's distributed cache (load balancing).
            cache.Clear();
            distributedCache?.RefreshAll(UmbCheckoutConfigurationCacheRefresher.UniqueId);
        }
    }
}
