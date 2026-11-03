using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using UmbCheckout.Shared.Notifications;
using Umbraco.Cms.Core.Events;

namespace UmbCheckout.Core.ManagementApi
{
    /// <summary>
    /// A save or delete refused because a notification handler cancelled it: the handler's message, for the person who
    /// tried. The backoffice error toast shows the ProblemDetails title and detail, so the title is short ("Save cancelled")
    /// and the detail is the message; screens with their own failure toast recognise it by <see cref="UmbCheckoutProblemDetails.CancelledType"/>.
    /// Reading the cancel clears it, so a later failure in the same request keeps its own response.
    /// </summary>
    public static class UmbCheckoutCancellation
    {
        /// <summary>The cancel message a notification handler gave in this request, or null; cleared once read.</summary>
        public static string? Message(HttpContext? httpContext)
            => httpContext?.RequestServices?.GetService<IEventMessagesFactory>()?.GetOrDefault().TakeCancellationMessage();

        /// <summary>A 400 for a cancelled save, with the handler's message as its detail, or null when no handler cancelled.</summary>
        public static IActionResult? Result(HttpContext? httpContext)
            => Result(httpContext, UmbCheckoutProblemDetails.SaveCancelledTitle);

        /// <summary>A 400 titled <paramref name="title"/> (e.g. <see cref="UmbCheckoutProblemDetails.DeleteCancelledTitle"/>) with the handler's message as its detail, or null when no handler cancelled.</summary>
        public static IActionResult? Result(HttpContext? httpContext, string title)
            => Message(httpContext) is { } message
                ? new BadRequestObjectResult(new ProblemDetails { Type = UmbCheckoutProblemDetails.CancelledType, Title = title, Detail = message, Status = StatusCodes.Status400BadRequest })
                : null;
    }
}
