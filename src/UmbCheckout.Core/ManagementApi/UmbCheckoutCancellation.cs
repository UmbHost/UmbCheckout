using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using UmbCheckout.Shared.Notifications;
using Umbraco.Cms.Core.Events;

namespace UmbCheckout.Core.ManagementApi
{
    /// <summary>
    /// A save or delete refused because a notification handler cancelled it: the handler's message, for the person who
    /// tried. The ProblemDetails title carries it because the backoffice error toast shows the title; screens with their own
    /// failure toast recognise it by <see cref="UmbCheckoutProblemDetails.CancelledType"/>.
    /// </summary>
    public static class UmbCheckoutCancellation
    {
        /// <summary>The cancel message a notification handler gave in this request, or null.</summary>
        public static string? Message(HttpContext? httpContext)
            => httpContext?.RequestServices?.GetService<IEventMessagesFactory>()?.GetOrDefault().CancellationMessage();

        /// <summary>A 400 with the handler's cancel message, or null when no handler cancelled.</summary>
        public static IActionResult? Result(HttpContext? httpContext)
            => Message(httpContext) is { } message
                ? new BadRequestObjectResult(new ProblemDetails { Type = UmbCheckoutProblemDetails.CancelledType, Title = message, Detail = message, Status = StatusCodes.Status400BadRequest })
                : null;
    }
}
