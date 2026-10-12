namespace UmbCheckout.Shared.Persistence
{
    /// <summary>
    /// A process-wide cache of one stored settings row (the stored entity, or "none saved yet"). Services map a fresh model from it on
    /// every read, so callers and Loaded handlers never share an instance. Cleared on save, and on every load-balanced server by the
    /// row's cache refresher.
    /// </summary>
    public sealed class SettingsCache<TEntity> where TEntity : class
    {
        private volatile Box? _box;

        private sealed record Box(TEntity? Value);

        /// <summary>The cached row (null when none is saved yet); false when nothing is cached.</summary>
        public bool TryGet(out TEntity? value)
        {
            var box = _box;
            value = box?.Value;
            return box is not null;
        }

        public void Set(TEntity? value) => _box = new Box(value);

        public void Clear() => _box = null;
    }
}
