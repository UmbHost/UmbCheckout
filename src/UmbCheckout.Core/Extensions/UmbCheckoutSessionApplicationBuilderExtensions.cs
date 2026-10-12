using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;

namespace UmbCheckout.Core.Extensions
{
    public static class UmbCheckoutSessionApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds ASP.NET Core session middleware unless the site already added it (an existing <c>app.UseSession()</c> in Program.cs
        /// keeps working; it is no longer needed). UmbCheckout adds this through an Umbraco pipeline filter.
        /// </summary>
        public static IApplicationBuilder UseUmbCheckoutSession(this IApplicationBuilder app) =>
            app.UseWhen(context => context.Features.Get<ISessionFeature>() is null, branch => branch.UseSession());
    }
}
