using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UmbCheckout.Testing;

/// <summary>
/// Source-level guard for obfuscated assemblies: EF finds migrations and snapshots by reflection (and by namespace), and maps entity
/// property names to columns, so every persistence type must carry [Obfuscation(Exclude = true)]. Run against a Debug build: the
/// obfuscator strips the attribute from Release output.
/// </summary>
public static class ObfuscationGuard
{
    public static IReadOnlyList<string> PersistenceTypesNotExcluded(Assembly assembly)
    {
        var types = assembly.GetTypes().Where(t => !t.Name.StartsWith('<')).ToList();
        var entities = types
            .Where(t => typeof(DbContext).IsAssignableFrom(t))
            .SelectMany(t => t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments()[0])
            .ToHashSet();

        return types
            .Where(t => IsPersistenceType(t) || entities.Contains(t))
            .Where(t => !t.GetCustomAttributes<ObfuscationAttribute>(inherit: false).Any(a => a.Exclude))
            .Select(t => t.FullName ?? t.Name)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();
    }

    private static bool IsPersistenceType(Type type) =>
        typeof(DbContext).IsAssignableFrom(type)
        || typeof(Migration).IsAssignableFrom(type)
        || typeof(ModelSnapshot).IsAssignableFrom(type)
        || typeof(IMigrationsAssembly).IsAssignableFrom(type)
        || type.GetInterfaces().Any(i => i.IsGenericType
            && (i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>) || i.GetGenericTypeDefinition().Name == "IDesignTimeDbContextFactory`1"));
}
