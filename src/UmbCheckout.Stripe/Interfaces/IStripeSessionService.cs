using Stripe.Checkout;
using UmbCheckout.Shared.Models;

namespace UmbCheckout.Stripe.Interfaces
{
    public interface IStripeSessionService
    {
        Session GetSession(string id);

        Task<Session> GetSessionAsync(string id);

        Session CreateSession(Basket basket);

        Task<Session> CreateSessionAsync(Basket basket);

        void ClearSession(string id);
        
        Task ClearSessionAsync(string id);
    }
}
