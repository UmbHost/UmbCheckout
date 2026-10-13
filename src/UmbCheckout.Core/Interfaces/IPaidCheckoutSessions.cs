namespace UmbCheckout.Core.Interfaces
{
    /// <summary>
    /// Checkout sessions the payment provider has reported paid (e.g. by webhook). A basket read clears a basket whose checkout
    /// session is paid, so a customer who paid but never returned to the success page doesn't keep the paid basket. A site can
    /// register its own; a payment provider replaces only <see cref="NoPaidCheckoutSessions"/>.
    /// </summary>
    public interface IPaidCheckoutSessions
    {
        /// <summary>True when the checkout session has been reported paid.</summary>
        Task<bool> IsPaidAsync(string checkoutSessionId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// The default: no payment provider records paid sessions, so nothing is cleared on read. A payment provider replaces only
    /// this default, never a store the site registered itself.
    /// </summary>
    public sealed class NoPaidCheckoutSessions : IPaidCheckoutSessions
    {
        /// <inheritdoc />
        public Task<bool> IsPaidAsync(string checkoutSessionId, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }
}
