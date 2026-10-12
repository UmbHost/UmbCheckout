using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Persistence.EFCore.Scoping;

namespace UmbCheckout.Testing;

public sealed class TestEfCoreScope<TContext> : IEfCoreScope<TContext> where TContext : DbContext
{
    private readonly TestEfCoreScopeProvider<TContext> _provider;
    private readonly TContext _context;
    private readonly IDbContextTransaction? _transaction;
    private bool _completed;
    private bool _childFailed;

    internal TestEfCoreScope(TestEfCoreScopeProvider<TContext> provider, TestEfCoreScope<TContext>? parent, Func<TContext> createContext)
    {
        _provider = provider;
        Parent = parent;
        if (parent is null)
        {
            _context = createContext();
            _transaction = _context.Database.BeginTransaction();
        }
        else
        {
            _context = parent._context;
        }
    }

    internal TestEfCoreScope<TContext>? Parent { get; }

    public Guid InstanceId { get; } = Guid.NewGuid();

    public int CreatedThreadId { get; } = Environment.CurrentManagedThreadId;

    public int Depth => Parent is null ? 0 : Parent.Depth + 1;

    public IScopeContext? ScopeContext { get; set; }

    public IScopedNotificationPublisher Notifications => _provider.Notifications;

    public RepositoryCacheMode RepositoryCacheMode => RepositoryCacheMode.Unspecified;

    public ILockingMechanism Locks => throw new NotSupportedException();

    public IsolatedCaches IsolatedCaches => throw new NotSupportedException();

    public Task<T> ExecuteWithContextAsync<T>(Func<TContext, Task<T>> method) => method(_context);

    public Task ExecuteWithContextAsync<T>(Func<TContext, Task> method) => method(_context);

    public bool Complete()
    {
        _completed = true;
        return true;
    }

    public void Dispose()
    {
        _provider.Exit(this);
        if (Parent is not null)
        {
            if (!_completed)
            {
                Parent.MarkFailed();
            }

            return;
        }

        var commit = _completed && !_childFailed;
        if (commit)
        {
            _transaction!.Commit();
        }
        else
        {
            _transaction!.Rollback();
        }

        _transaction.Dispose();
        _context.Dispose();
        _provider.Notifications.ScopeExit(commit);
    }

    private void MarkFailed()
    {
        _childFailed = true;
        Parent?.MarkFailed();
    }

    public void ReadLock(params int[] lockIds) => throw new NotSupportedException();

    public void WriteLock(params int[] lockIds) => throw new NotSupportedException();

    public void WriteLock(TimeSpan timeout, int lockId) => throw new NotSupportedException();

    public void ReadLock(TimeSpan timeout, int lockId) => throw new NotSupportedException();

    public void EagerWriteLock(params int[] lockIds) => throw new NotSupportedException();

    public void EagerWriteLock(TimeSpan timeout, int lockId) => throw new NotSupportedException();

    public void EagerReadLock(TimeSpan timeout, int lockId) => throw new NotSupportedException();

    public void EagerReadLock(params int[] lockIds) => throw new NotSupportedException();
}
