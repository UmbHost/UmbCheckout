using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UmbCheckout.Shared.Persistence;

/// <summary>
/// Offers EF only the migrations and model snapshot generated for the active database provider. Each context keeps one
/// migration set per provider (namespaces ending in <see cref="SqlServerNamespaceSuffix"/> / <see cref="SqliteNamespaceSuffix"/>),
/// because a set generated for SQL Server would create the wrong column types on SQLite.
/// </summary>
public sealed class ProviderMigrationsAssembly : IMigrationsAssembly
{
    public const string SqlServerNamespaceSuffix = ".Migrations.SqlServer";
    public const string SqliteNamespaceSuffix = ".Migrations.Sqlite";

    private readonly Type _contextType;
    private readonly string _namespaceSuffix;
    private readonly IMigrationsIdGenerator _idGenerator;
    private IReadOnlyDictionary<string, TypeInfo>? _migrations;
    private ModelSnapshot? _modelSnapshot;
    private bool _modelSnapshotLoaded;

    public ProviderMigrationsAssembly(ICurrentDbContext currentContext, IMigrationsIdGenerator idGenerator)
    {
        _contextType = currentContext.Context.GetType();
        _idGenerator = idGenerator;
        _namespaceSuffix = NamespaceSuffixFor(currentContext.Context.Database.ProviderName);
        Assembly = _contextType.Assembly;
    }

    public Assembly Assembly { get; }

    public IReadOnlyDictionary<string, TypeInfo> Migrations => _migrations ??= LoadMigrations();

    public ModelSnapshot? ModelSnapshot
    {
        get
        {
            if (!_modelSnapshotLoaded)
            {
                var type = Assembly.DefinedTypes.FirstOrDefault(t => t.IsSubclassOf(typeof(ModelSnapshot)) && IsForThisContextAndProvider(t));
                _modelSnapshot = type is null ? null : CreateInstance<ModelSnapshot>(type);
                _modelSnapshotLoaded = true;
            }

            return _modelSnapshot;
        }
    }

    public static string NamespaceSuffixFor(string? providerName) => providerName switch
    {
        "Microsoft.EntityFrameworkCore.SqlServer" => SqlServerNamespaceSuffix,
        "Microsoft.EntityFrameworkCore.Sqlite" => SqliteNamespaceSuffix,
        _ => throw new NotSupportedException($"UmbCheckout supports SQL Server and SQLite, not '{providerName}'."),
    };

    public string? FindMigrationId(string nameOrId) => Migrations.Keys.FirstOrDefault(id => _idGenerator.IsValidId(nameOrId)
        ? string.Equals(id, nameOrId, StringComparison.OrdinalIgnoreCase)
        : string.Equals(_idGenerator.GetName(id), nameOrId, StringComparison.OrdinalIgnoreCase));

    public Migration CreateMigration(TypeInfo migrationClass, string activeProvider)
    {
        var migration = CreateInstance<Migration>(migrationClass);
        migration.ActiveProvider = activeProvider;
        return migration;
    }

    private static T CreateInstance<T>(TypeInfo type) where T : class
        => Activator.CreateInstance(type.AsType()) as T
           ?? throw new InvalidOperationException($"Could not create an instance of {type.FullName} as {typeof(T).Name}.");

    private IReadOnlyDictionary<string, TypeInfo> LoadMigrations()
    {
        var migrations = new SortedList<string, TypeInfo>(StringComparer.Ordinal);
        foreach (var type in Assembly.DefinedTypes.Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Migration)) && IsForThisContextAndProvider(t)))
        {
            var id = type.GetCustomAttribute<MigrationAttribute>()?.Id;
            if (id is not null)
            {
                migrations.Add(id, type);
            }
        }

        return migrations;
    }

    private bool IsForThisContextAndProvider(TypeInfo type) =>
        type.GetCustomAttribute<DbContextAttribute>()?.ContextType == _contextType
        && type.Namespace?.EndsWith(_namespaceSuffix, StringComparison.Ordinal) == true;
}
