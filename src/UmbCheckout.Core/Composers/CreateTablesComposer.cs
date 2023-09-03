using UmbCheckout.Core.NotificationHandlers;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace UmbCheckout.Core.Composers
{
    /// <summary>
    /// Registers the database table migrations
    /// </summary>
    internal class CreateTablesComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.AddNotificationHandler<UmbracoApplicationStartingNotification, RunUmbCheckoutConfigurationMigration>();
        }
    }
}
