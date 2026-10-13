using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Persistence.Sqlite.Services;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace UmbCheckout.Tests.Infrastructure
{
    /// <summary>
    /// A real NPoco database on in-memory SQLite (one per test, no Docker), with an <see cref="IScopeProvider"/> whose scopes all
    /// use it. Published scope notifications are recorded in <see cref="Published"/>.
    /// </summary>
    public sealed class SqliteTestDatabase : IDisposable
    {
        private readonly SqliteConnection _keepAlive;
        private readonly string _connectionString;

        public SqliteTestDatabase()
        {
            _connectionString = $"Data Source=file:umbcheckout-{Guid.NewGuid():N}?mode=memory&cache=shared";
            _keepAlive = new SqliteConnection(_connectionString);
            _keepAlive.Open();

            var syntax = new SqliteSyntaxProvider(Options.Create(new GlobalSettings()), NullLogger<SqliteSyntaxProvider>.Instance);
            SqlContext = new SqlContext(syntax, DatabaseType.SQLite, new PocoDataFactory(new NPoco.MapperCollection()), new Umbraco.Cms.Infrastructure.Persistence.Mappers.MapperCollection(() => Enumerable.Empty<BaseMapper>()));

            var notifications = new Mock<IScopedNotificationPublisher>();
            notifications.Setup(n => n.Publish(It.IsAny<INotification>())).Callback<INotification>(Published.Add);

            var scope = new Mock<IScope>();
            Database = Open();
            scope.Setup(s => s.Database).Returns(Database);
            scope.Setup(s => s.SqlContext).Returns(SqlContext);
            scope.Setup(s => s.Notifications).Returns(notifications.Object);

            var provider = new Mock<IScopeProvider>();
            provider.SetReturnsDefault<IScope>(scope.Object);
            provider.SetReturnsDefault<ICoreScope>(scope.Object);
            provider.Setup(p => p.SqlContext).Returns(SqlContext);
            ScopeProvider = provider.Object;
        }

        public ISqlContext SqlContext { get; }

        /// <summary>The database every scope uses.</summary>
        public IUmbracoDatabase Database { get; }

        public IScopeProvider ScopeProvider { get; }

        public List<INotification> Published { get; } = new();

        /// <summary>A new database connection (each scope gets its own, as on a site).</summary>
        public IUmbracoDatabase Open()
            => new UmbracoDatabase(_connectionString, SqlContext, SqliteFactory.Instance, NullLogger<UmbracoDatabase>.Instance, null, null!);

        /// <summary>Runs migration steps (as a plan would, without the plan's state table).</summary>
        public void Migrate(params Func<IMigrationContext, MigrationBase>[] steps)
        {
            var context = new Mock<IMigrationContext>();
            context.Setup(c => c.Database).Returns(Database);
            context.Setup(c => c.SqlContext).Returns(SqlContext);
            context.Setup(c => c.Logger).Returns(NullLogger<IMigrationContext>.Instance);
            context.Setup(c => c.Plan).Returns(new MigrationPlan("test"));
            context.SetupProperty(c => c.BuildingExpression);
            foreach (var step in steps)
            {
                step(context.Object).Run();
            }
        }

        public int Execute(string sql, params object[] args)
        {
            using var database = Open();
            return database.Execute(sql, args);
        }

        public T Scalar<T>(string sql, params object[] args)
        {
            using var database = Open();
            return database.ExecuteScalar<T>(sql, args);
        }

        public void Dispose()
        {
            Database.Dispose();
            _keepAlive.Dispose();
        }
    }
}
