using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Persistence.Sqlite.Services;
using Umbraco.Cms.Persistence.SqlServer.Services;
using UmbCheckout.Testing;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace UmbCheckout.Tests.Infrastructure
{
    /// <summary>Which database engine a test runs against. SQL Server runs in a Testcontainers container and needs Docker.</summary>
    public enum TestProvider
    {
        Sqlite,
        SqlServer
    }

    /// <summary>
    /// A real NPoco database (one per test): in-memory SQLite (no Docker) or a new database on the shared SQL Server container
    /// (<see cref="SqlServerContainerFixture"/>, Docker; those tests are <c>[Trait("Category", "Integration")]</c>), with an
    /// <see cref="IScopeProvider"/> whose scopes all use it. Published scope notifications are recorded in <see cref="Published"/>.
    /// </summary>
    public sealed class NPocoTestDatabase : IDisposable
    {
        private readonly SqliteConnection? _keepAlive;
        private readonly string _connectionString;
        private readonly DbProviderFactory _factory;
        
        /// <summary>In-memory SQLite.</summary>
        public NPocoTestDatabase() : this(TestProvider.Sqlite, null)
        {
        }

        private NPocoTestDatabase(TestProvider engine, string? sqlServerConnectionString)
        {
            Provider = engine;
            if (engine == TestProvider.Sqlite)
            {
                _connectionString = $"Data Source=file:umbcheckout-{Guid.NewGuid():N}?mode=memory&cache=shared";
                _keepAlive = new SqliteConnection(_connectionString);
                _keepAlive.Open();
                _factory = SqliteFactory.Instance;
                var syntax = new SqliteSyntaxProvider(Options.Create(new GlobalSettings()), NullLogger<SqliteSyntaxProvider>.Instance);
                SqlContext = new SqlContext(syntax, DatabaseType.SQLite, new PocoDataFactory(new NPoco.MapperCollection()), new Umbraco.Cms.Infrastructure.Persistence.Mappers.MapperCollection(() => Enumerable.Empty<BaseMapper>()));
            }
            else
            {
                _connectionString = sqlServerConnectionString.Required();
                _factory = SqlClientFactory.Instance;
                var syntax = new SqlServerSyntaxProvider(Options.Create(new GlobalSettings()));
                SqlContext = new SqlContext(syntax, DatabaseType.SqlServer2012, new PocoDataFactory(new NPoco.MapperCollection()), new Umbraco.Cms.Infrastructure.Persistence.Mappers.MapperCollection(() => Enumerable.Empty<BaseMapper>()));
            }

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

        public TestProvider Provider { get; }

        /// <summary>A new, empty database on the shared SQL Server container (starts the container on first use).</summary>
        public static async Task<NPocoTestDatabase> SqlServerAsync(SqlServerContainerFixture sqlServer)
            => new NPocoTestDatabase(TestProvider.SqlServer, await sqlServer.CreateDatabaseAsync());

        /// <summary>Blocking form of <see cref="SqlServerAsync"/>, for the constructor of a test class.</summary>
        public static NPocoTestDatabase SqlServer(SqlServerContainerFixture sqlServer) => SqlServerAsync(sqlServer).GetAwaiter().GetResult();

        public ISqlContext SqlContext { get; }

        /// <summary>The database every scope uses.</summary>
        public IUmbracoDatabase Database { get; }

        public IScopeProvider ScopeProvider { get; }

        public List<INotification> Published { get; } = new();

        /// <summary>A new database connection (each scope gets its own, as on a site).</summary>
        public IUmbracoDatabase Open()
            => new UmbracoDatabase(_connectionString, SqlContext, _factory, NullLogger<UmbracoDatabase>.Instance, null,
                new DatabaseSchemaCreatorFactory(NullLogger<DatabaseSchemaCreator>.Instance, NullLoggerFactory.Instance, Mock.Of<IUmbracoVersion>(),
                    Mock.Of<IEventAggregator>(), Mock.Of<IOptionsMonitor<InstallDefaultDataSettings>>()));

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
            _keepAlive?.Dispose();
        }
    }
}
