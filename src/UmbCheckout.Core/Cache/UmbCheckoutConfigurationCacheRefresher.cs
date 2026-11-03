using UmbCheckout.Shared.Persistence;
using Umbraco.Cms.Core.Cache;
using UmbCheckoutConfiguration = UmbCheckout.Core.Pocos.UmbCheckoutConfiguration;

namespace UmbCheckout.Core.Cache
{
    /// <summary>
    /// Clears the cached UmbCheckout configuration on every server when it is saved on one (Umbraco's distributed cache calls it).
    /// </summary>
    internal sealed class UmbCheckoutConfigurationCacheRefresher(SettingsCache<UmbCheckoutConfiguration> cache) : ICacheRefresher
    {
        public static readonly Guid UniqueId = new("6a6a9d64-3c4f-4b8e-9b0e-6a1d2c7f4e51");

        public Guid RefresherUniqueId => UniqueId;

        public string Name => "UmbCheckout configuration cache refresher";

        public void RefreshAll() => cache.Clear();

        public void Refresh(int id) => cache.Clear();

        public void Remove(int id) => cache.Clear();

        public void Refresh(Guid id) => cache.Clear();
    }
}
