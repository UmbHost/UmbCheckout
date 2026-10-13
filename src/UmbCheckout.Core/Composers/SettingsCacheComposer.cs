using Microsoft.Extensions.DependencyInjection;
using UmbCheckout.Core.Cache;
using UmbCheckout.Shared.Notifications.Configuration;
using UmbCheckout.Shared.Persistence;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;
using UmbCheckoutConfiguration = UmbCheckout.Core.Pocos.UmbCheckoutConfiguration;

namespace UmbCheckout.Core.Composers
{
    /// <summary>
    /// One cached configuration row per process: cleared on save here, after the outermost commit by the Saved handler (which also
    /// tells every other server), and on every other server by its cache refresher.
    /// </summary>
    public class SettingsCacheComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<SettingsCache<UmbCheckoutConfiguration>>();
            builder.AddNotificationHandler<OnConfigurationSavedNotification, UmbCheckoutConfigurationSavedCacheHandler>();
            builder.CacheRefreshers().Add<UmbCheckoutConfigurationCacheRefresher>();
        }
    }
}
