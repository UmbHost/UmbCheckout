using Microsoft.Extensions.Logging;

namespace UmbCheckout.Tests.Helpers
{
    /// <summary>
    /// Records log entries. Moq can't proxy <see cref="ILogger{T}"/> for an internal <typeparamref name="T"/> (strong-named Logging.Abstractions).
    /// </summary>
    public sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = new();

        public int Count(LogLevel level) => Entries.Count(entry => entry.Level == level);

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Entries.Add((logLevel, formatter(state, exception)));

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
