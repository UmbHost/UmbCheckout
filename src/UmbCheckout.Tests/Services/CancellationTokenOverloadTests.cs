using System.Reflection;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Services;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Services;
using Xunit;

namespace UmbCheckout.Tests.Services
{
    /// <summary>
    /// Every async member of the public shop interfaces has an overload taking a <see cref="CancellationToken"/>. The overloads are
    /// default interface methods calling the old ones, so a site's own implementation still compiles; UmbCheckout's implementations
    /// implement them, so the token isn't dropped.
    /// </summary>
    public class CancellationTokenOverloadTests
    {
        public static TheoryData<Type, Type> Interfaces => new()
        {
            { typeof(ISessionService), typeof(SessionService) },
            { typeof(IBasketService), typeof(BasketService) },
            { typeof(ICurrencyService), typeof(CurrencyService) },
            { typeof(IBasketContext), typeof(UmbracoBasketContext) },
            { typeof(IUmbCheckoutStripeClientFactory), typeof(UmbCheckoutStripeClientFactory) },
            { typeof(IStripeLineItemPricingService), typeof(StripeLineItemPricingService) },
            { typeof(IStripeBasketPricingService), typeof(StripeBasketPricingService) },
            { typeof(IStripeShippingOptionsService), typeof(StripeDefaultShippingOptionsService) },
            { typeof(IStripeShippingRateApiService), typeof(StripeShippingRateApiService) },
            { typeof(IStripeSessionService), typeof(StripeSessionService) },
        };

        [Theory]
        [MemberData(nameof(Interfaces))]
        public void Every_async_member_has_a_token_overload_that_is_a_default_interface_method(Type contract, Type implementation)
        {
            var map = implementation.GetInterfaceMap(contract);
            var missing = new List<string>();
            foreach (var method in TokenlessAsyncMembers(contract))
            {
                var parameters = method.GetParameters().Select(p => p.ParameterType).Append(typeof(CancellationToken)).ToArray();
                var overload = contract.GetMethod(method.Name, parameters);
                if (overload == null || overload.IsAbstract || overload.ReturnType != method.ReturnType)
                {
                    missing.Add($"{method.Name}({string.Join(", ", parameters.Select(p => p.Name))}) as a default interface method");
                    continue;
                }

                var target = map.TargetMethods[Array.IndexOf(map.InterfaceMethods, overload)];
                if (target.DeclaringType != implementation)
                {
                    missing.Add($"{implementation.Name} implementing {method.Name}({string.Join(", ", parameters.Select(p => p.Name))})");
                }
            }

            Assert.True(missing.Count == 0, $"{contract.Name} is missing: {string.Join("; ", missing)}");
        }

        private static IEnumerable<MethodInfo> TokenlessAsyncMembers(Type contract) =>
            contract.GetMethods().Where(m => typeof(Task).IsAssignableFrom(m.ReturnType) && m.GetParameters().All(p => p.ParameterType != typeof(CancellationToken)));
    }
}
