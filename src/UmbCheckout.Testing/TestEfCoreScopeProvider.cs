using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Persistence.EFCore.Scoping;

namespace UmbCheckout.Testing;

/// <summary>
/// A real-database stand-in for Umbraco's EF Core scope provider: the outermost scope owns one context and one transaction,
/// nested scopes share them, and the transaction commits only if every scope completed.
/// </summary>
public sealed class TestEfCoreScopeProvider<TContext> : IEFCoreScopeProvider<TContext> where TContext : DbContext
{
    private readonly Func<TContext> _createContext;
    private TestEfCoreScope<TContext>? _ambient;

    public TestEfCoreScopeProvider(Func<TContext> createContext) => _createContext = createContext;

    public RecordingNotificationPublisher Notifications { get; } = new();

    public IScopeContext? AmbientScopeContext => null;

    public IEfCoreScope<TContext> CreateScope(RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified, bool? scopeFileSystems = null)
    {
        var scope = new TestEfCoreScope<TContext>(this, _ambient, _createContext);
        _ambient = scope;
        return scope;
    }

    public IEfCoreScope<TContext> CreateDetachedScope(RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified, bool? scopeFileSystems = null) =>
        throw new NotSupportedException();

    public void AttachScope(IEfCoreScope<TContext> other) => throw new NotSupportedException();

    public IEfCoreScope<TContext> DetachScope() => throw new NotSupportedException();

    internal void Exit(TestEfCoreScope<TContext> scope) => _ambient = scope.Parent;
}
