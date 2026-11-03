using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using UmbCheckout.Shared.Notifications;
using UmbCheckout.Testing;
using Umbraco.Cms.Core.Events;

namespace UmbCheckout.Tests.Api
{
    /// <summary>A request whose event messages hold a handler's cancel message (or none), as Umbraco keeps them per request.</summary>
    internal static class CancelledRequest
    {
        public static T With<T>(T controller, string? message) where T : ControllerBase
        {
            var factory = new TestEventMessagesFactory();
            if (message != null)
            {
                factory.Messages.Add(new EventMessage("Test", message, EventMessageType.Error));
                factory.Messages.MarkCancelled();
            }

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = new ServiceCollection().AddSingleton<IEventMessagesFactory>(factory).BuildServiceProvider()
                }
            };
            return controller;
        }
    }
}
