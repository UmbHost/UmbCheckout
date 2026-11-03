using System.Reflection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Xunit;

namespace UmbCheckout.Tests.Persistence
{
    /// <summary>
    /// A fresh site with uSync's import-on-first-boot needs UmbCheckout's tables before the import. uSync imports on
    /// <see cref="UmbracoApplicationStartedNotification"/>; UmbCheckout's migrations run on the earlier
    /// <see cref="UmbracoApplicationStartingNotification"/>, so the tables exist first.
    /// </summary>
    public class MigrationOrderTests
    {
        private static IEnumerable<Type> Migrations() =>
            new[] { typeof(UmbCheckout.Core.Services.ConfigurationService), typeof(UmbCheckout.Stripe.Services.StripeSettingsService) }
                .Select(t => t.Assembly)
                .SelectMany(a => a.GetTypes())
                .Where(t => t.Name.StartsWith("RunUmbCheckout", StringComparison.Ordinal) && t.Name.EndsWith("Migration", StringComparison.Ordinal));

        [Fact]
        public void The_migrations_run_when_the_application_is_starting()
        {
            var migrations = Migrations().ToList();

            Assert.NotEmpty(migrations);
            Assert.All(migrations, m =>
            {
                Assert.True(typeof(INotificationHandler<UmbracoApplicationStartingNotification>).IsAssignableFrom(m), m.Name);
                Assert.False(typeof(INotificationHandler<UmbracoApplicationStartedNotification>).IsAssignableFrom(m), m.Name);
            });
        }

        [Fact]
        public void uSyncs_first_boot_import_runs_after_the_application_has_started()
        {
            var uSync = Assembly.Load("uSync.BackOffice");
            var handlers = uSync.GetTypes().Where(t => t.Namespace == "uSync.BackOffice.Boot" && t.Name.StartsWith("FirstBoot", StringComparison.Ordinal) && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))).ToList();

            Assert.NotEmpty(handlers);
            Assert.All(handlers, h =>
            {
                Assert.True(typeof(INotificationHandler<UmbracoApplicationStartedNotification>).IsAssignableFrom(h), h.Name);
                Assert.False(typeof(INotificationHandler<UmbracoApplicationStartingNotification>).IsAssignableFrom(h), h.Name);
            });
        }
    }
}
