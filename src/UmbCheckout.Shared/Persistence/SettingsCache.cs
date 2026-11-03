namespace UmbCheckout.Shared.Persistence
{
    /// <summary>
    /// A process-wide cache of one stored settings row (the stored entity, or "none saved yet"). Services map a fresh model from it on
    /// every read, so callers never share (or change) the cached instance. Cleared on save, and on every load-balanced server by the
    /// row's cache refresher.
    /// </summary>
    public sealed class SettingsCache<TEntity> where TEntity : class
    {
        private readonly object _lock = new();
        private Box? _box;
        private long _generation;

        private sealed record Box(TEntity? Value);

        /// <summary>Read before querying the database, and handed back to <see cref="TrySet"/>.</summary>
        public long Generation
        {
            get
            {
                lock (_lock)
                {
                    return _generation;
                }
            }
        }

        /// <summary>The cached row (null when none is saved yet); false when nothing is cached.</summary>
        public bool TryGet(out TEntity? value)
        {
            Box? box;
            lock (_lock)
            {
                box = _box;
            }

            value = box?.Value;
            return box is not null;
        }

        /// <summary>
        /// Caches a row read from the database, unless the cache was cleared since <paramref name="generation"/> was taken:
        /// a read that started before a save would otherwise put the old row back.
        /// </summary>
        public void TrySet(TEntity? value, long generation)
        {
            lock (_lock)
            {
                if (generation == _generation)
                {
                    _box = new Box(value);
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _box = null;
                _generation++;
            }
        }
    }
}
