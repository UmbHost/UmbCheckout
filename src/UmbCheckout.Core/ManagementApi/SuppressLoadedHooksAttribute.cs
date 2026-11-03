using Microsoft.AspNetCore.Mvc.Filters;
using UmbCheckout.Shared.Notifications;

namespace UmbCheckout.Core.ManagementApi
{
    /// <summary>
    /// For backoffice APIs that load data for editing and save it back: their reads return what is stored, without
    /// <c>…LoadedNotification</c>s, so a site's display changes are neither shown to editors as stored values nor saved back.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class SuppressLoadedHooksAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            using (UmbCheckoutHooks.SuppressLoaded())
            {
                await next();
            }
        }
    }
}
