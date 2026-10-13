using UmbCheckout.Shared.Persistence;
using Umbraco.Cms.Core.Cache;
using UmbCheckoutConfiguration = UmbCheckout.Core.Pocos.UmbCheckoutConfiguration;

namespace UmbCheckout.Core.Cache
{
    /// <summary>
    /// Clears the cached UmbCheckout configuration on every server when it is saved on one (Umbraco's distributed cache calls it).
    /// </summary>
    internal sealed class UmbCheckoutConfigurationCacheRefresher : ICacheRefresher
    {
        /// <summary>The same id as UmbCheckout for Umbraco 17.</summary>
        public static readonly Guid UniqueId = new("6a6a9d64-3c4f-4b8e-9b0e-6a1d2c7f4e51");

        private readonly SettingsCache<UmbCheckoutConfiguration> _cache;

        public UmbCheckoutConfigurationCacheRefresher(SettingsCache<UmbCheckoutConfiguration> cache)
        {
            _cache = cache;
        }

        public Guid RefresherUniqueId => UniqueId;

        public string Name => "UmbCheckout configuration cache refresher";

        public void RefreshAll() => _cache.Clear();

        public void Refresh(int id) => _cache.Clear();

        public void Remove(int id) => _cache.Clear();

        public void Refresh(Guid id) => _cache.Clear();
    }
}
