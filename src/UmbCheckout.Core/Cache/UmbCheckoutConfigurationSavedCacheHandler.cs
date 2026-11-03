using UmbCheckout.Shared.Notifications.Configuration;
using UmbCheckout.Shared.Persistence;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using UmbCheckoutConfiguration = UmbCheckout.Core.Pocos.UmbCheckoutConfiguration;

namespace UmbCheckout.Core.Cache
{
    /// <summary>
    /// Clears the cached configuration once a save has committed. Umbraco publishes the scoped Saved notification when the outermost
    /// scope completes (and drops it on a rollback), so this also covers a save inside someone else's scope (a uSync import), where the
    /// service's own clear runs before the commit.
    /// </summary>
    internal sealed class UmbCheckoutConfigurationSavedCacheHandler : INotificationHandler<OnConfigurationSavedNotification>
    {
        private readonly SettingsCache<UmbCheckoutConfiguration> _cache;
        private readonly DistributedCache? _distributedCache;

        public UmbCheckoutConfigurationSavedCacheHandler(SettingsCache<UmbCheckoutConfiguration> cache, DistributedCache? distributedCache = null)
        {
            _cache = cache;
            _distributedCache = distributedCache;
        }

        public void Handle(OnConfigurationSavedNotification notification)
        {
            // This server's copy now, every other server's through Umbraco's distributed cache (load balancing).
            _cache.Clear();
            _distributedCache?.RefreshAll(UmbCheckoutConfigurationCacheRefresher.UniqueId);
        }
    }
}
