namespace UmbCheckout.Core.ManagementApi
{
    /// <summary>
    /// A page of a cursor-paged list (Stripe). Deliberate deviation from PagedViewModel: Stripe cannot supply a reliable total.
    /// </summary>
    public class CursorPagedViewModel<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

        public bool HasMore { get; set; }

        public string? NextCursor { get; set; }
    }
}
