using Microsoft.EntityFrameworkCore;
using Moq;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Scoping;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;
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

    /// <summary>Umbraco's scope accessor as a service sees it: an ambient scope while any scope from this provider is open.</summary>
    public IScopeAccessor ScopeAccessor => new AmbientScopeAccessor(this);

    private sealed class AmbientScopeAccessor(TestEfCoreScopeProvider<TContext> provider) : IScopeAccessor
    {
        private static readonly IScope OpenScope = new Mock<IScope>().Object;

        public IScope? AmbientScope => provider._ambient is null ? null : OpenScope;
    }

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
