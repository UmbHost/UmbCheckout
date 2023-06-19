using UmbCheckout.Stripe.Models;

namespace UmbCheckout.Stripe.Interfaces
{
    public interface IStripeShippingRateDatabaseService
    {
        Task<IEnumerable<ShippingRate>> GetShippingRates();

        Task<ShippingRate?> GetShippingRate(long id);

        Task<ShippingRate?> UpdateShippingRate(ShippingRate shippingRate);

        Task<bool> DeleteShippingRate(long id);
    }
}
