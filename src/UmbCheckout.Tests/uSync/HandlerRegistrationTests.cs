using Microsoft.Extensions.DependencyInjection;
using Moq;
using UmbCheckout.Shared.Notifications.Configuration;
using UmbCheckout.Stripe.Notifications;
using UmbCheckout.Stripe.uSync.Handlers;
using UmbCheckout.uSync.Handlers;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Xunit;

namespace UmbCheckout.Tests.uSync
{
    /// <summary>Saving each setting writes its uSync file: the save handlers are registered (the Stripe settings one never was).</summary>
    public class HandlerRegistrationTests
    {
        private static IServiceCollection Compose(IComposer composer)
        {
            var services = new ServiceCollection();
            var builder = new Mock<IUmbracoBuilder>();
            builder.SetupGet(b => b.Services).Returns(services);
            composer.Compose(builder.Object);
            return services;
        }

        private static bool Registered<TNotification, THandler>(IServiceCollection services) where TNotification : INotification =>
            services.Any(d => d.ServiceType == typeof(INotificationHandler<TNotification>) && d.ImplementationType == typeof(THandler));

        [Fact]
        public void Saving_the_Stripe_settings_writes_their_uSync_file()
        {
            var services = Compose(new UmbCheckout.Stripe.uSync.Composers.RegisterNotifications());

            Assert.True(Registered<OnStripeSettingsSavedNotification, StripeSettingsHandler>(services));
        }

        [Fact]
        public void Saving_a_shipping_rate_writes_its_uSync_file()
        {
            var services = Compose(new UmbCheckout.Stripe.uSync.Composers.RegisterNotifications());

            Assert.True(Registered<OnShippingRateSavedNotification, ShippingRateHandler>(services));
            Assert.True(Registered<OnShippingRateDeletedNotification, ShippingRateHandler>(services));
        }

        [Fact]
        public void Saving_the_configuration_writes_its_uSync_file()
        {
            var services = Compose(new UmbCheckout.uSync.Composers.RegisterNotifications());

            Assert.True(Registered<OnConfigurationSavedNotification, ConfigurationHandler>(services));
        }
    }
}
