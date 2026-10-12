namespace UmbCheckout.Shared.Notifications
{
    /// <summary>
    /// Turns UmbCheckout's data hooks off for the code inside a <c>using</c> block (and everything it awaits), for code that works
    /// on stored data rather than on what a site shows: UmbCheckout uses it when it charges or changes a basket, when the
    /// backoffice loads data to edit it, for uSync, and when checkout records its Stripe session on the basket.
    /// </summary>
    public static class UmbCheckoutHooks
    {
        private static readonly AsyncLocal<bool> Loaded = new();
        private static readonly AsyncLocal<bool> Saving = new();

        /// <summary>Whether reads skip the <c>…LoadedNotification</c>s and return what is stored.</summary>
        public static bool LoadedSuppressed => Loaded.Value;

        /// <summary>Whether writes skip the <c>…SavingNotification</c>s (and so can't be cancelled by a handler).</summary>
        public static bool SavingSuppressed => Saving.Value;

        /// <summary>Reads inside the scope return what is stored, without <c>…LoadedNotification</c>s.</summary>
        public static IDisposable SuppressLoaded() => new Scope(Loaded);

        /// <summary>Writes inside the scope skip <c>…SavingNotification</c>s. Use only for bookkeeping a handler mustn't refuse.</summary>
        public static IDisposable SuppressSaving() => new Scope(Saving);

        /// <summary>Runs a read without <c>…LoadedNotification</c>s and returns what is stored.</summary>
        public static async Task<T> ReadStoredAsync<T>(Func<Task<T>> read)
        {
            using (SuppressLoaded())
            {
                return await read();
            }
        }

        private sealed class Scope : IDisposable
        {
            private readonly AsyncLocal<bool> _flag;
            private readonly bool _previous;

            public Scope(AsyncLocal<bool> flag)
            {
                _flag = flag;
                _previous = flag.Value;
                flag.Value = true;
            }

            public void Dispose() => _flag.Value = _previous;
        }
    }
}
