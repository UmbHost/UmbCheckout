using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace UmbCheckout.Tests.Infrastructure
{
    /// <summary>An in-memory ISession for services that keep the basket in the HTTP session.</summary>
    public sealed class InMemorySession : ISession
    {
        private readonly Dictionary<string, byte[]> _values = new();

        public bool IsAvailable => true;
        public string Id => "session-1";
        public IEnumerable<string> Keys => _values.Keys;
        public void Clear() => _values.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _values.Remove(key);
        public void Set(string key, byte[] value) => _values[key] = value;
        public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value) => _values.TryGetValue(key, out value);
    }
}
