using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Stripe.Composers;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.NotificationHandlers;
using UmbCheckout.Stripe.Notifications.Webhooks;
using UmbCheckout.Stripe.Services;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Xunit;

namespace UmbCheckout.Tests.Composers
{
    /// <summary>Group 5: Stripe's paid-session store replaces Core's "none paid" default whichever composer runs first, never a site's own.</summary>
    public sealed class PaidCheckoutSessionsComposerTests
    {
        /// <summary>As Core's RegisterServicesComposer.</summary>
        private static void Core(IServiceCollection services) => services.TryAddSingleton<IPaidCheckoutSessions, NoPaidCheckoutSessions>();

        private static IUmbracoBuilder Builder(IServiceCollection services)
        {
            var builder = new Mock<IUmbracoBuilder>();
            builder.SetupGet(b => b.Services).Returns(services);
            return builder.Object;
        }

        private static ServiceCollection Compose(bool coreFirst)
        {
            var services = new ServiceCollection();
            if (coreFirst)
            {
                Core(services);
            }

            new PaidCheckoutSessionsComposer().Compose(Builder(services));
            if (!coreFirst)
            {
                Core(services);
            }

            return services;
        }

        /// <summary>The Stripe store's own dependencies aren't registered here: a stand-in replaces it.</summary>
        private static IServiceProvider WithStripeStoreStandIn(ServiceCollection services)
        {
            services.RemoveAll<IStripePaidCheckoutSessions>();
            services.AddSingleton(Mock.Of<IStripePaidCheckoutSessions>());
            return services.BuildServiceProvider();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Basket_reads_ask_the_Stripe_store(bool coreFirst)
        {
            var services = Compose(coreFirst);

            Assert.Single(services, d => d.ServiceType == typeof(IPaidCheckoutSessions));
            Assert.Contains(services, d => d.ServiceType == typeof(IStripePaidCheckoutSessions) && d.ImplementationType == typeof(StripePaidCheckoutSessions));
            Assert.IsNotType<NoPaidCheckoutSessions>(WithStripeStoreStandIn(services).GetRequiredService<IPaidCheckoutSessions>());
        }

        private sealed class SitesOwn : IPaidCheckoutSessions
        {
            public Task<bool> IsPaidAsync(string checkoutSessionId, CancellationToken cancellationToken = default) => Task.FromResult(true);
        }

        [Fact]
        public void A_store_the_site_registered_itself_is_kept()
        {
            // The package must stay extendable: Stripe replaces only Core's "none paid" default.
            var services = new ServiceCollection();
            services.AddSingleton<IPaidCheckoutSessions>(new SitesOwn());

            new PaidCheckoutSessionsComposer().Compose(Builder(services));
            Core(services);

            Assert.IsType<SitesOwn>(WithStripeStoreStandIn(services).GetRequiredService<IPaidCheckoutSessions>());
        }

        [Fact]
        public void The_webhook_notifications_are_handled()
        {
            var services = Compose(coreFirst: true);

            Assert.Contains(services, d => d.ServiceType == typeof(INotificationAsyncHandler<OnCheckoutSessionCompletedNotification>) && d.ImplementationType == typeof(RecordPaidCheckoutSessionHandler));
            Assert.Contains(services, d => d.ServiceType == typeof(INotificationAsyncHandler<OnPaymentSuccessNotification>) && d.ImplementationType == typeof(RecordPaidCheckoutSessionHandler));
        }

        [Fact]
        public void The_table_is_created_on_every_site()
        {
            // Open: the tables composer registers the plan's runner, which reads no licence.
            var services = new ServiceCollection();
            var composer = (IComposer)Activator.CreateInstance(typeof(PaidCheckoutSessionsComposer).Assembly.GetType("UmbCheckout.Stripe.Composers.CreateTablesComposer", throwOnError: true)!)!;

            composer.Compose(Builder(services));

            Assert.Contains(services, d => d.ImplementationType == typeof(RunUmbCheckoutStripePaidSessionsMigration));
        }
    }
}
