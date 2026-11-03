using Umbraco.Cms.Core.Events;

namespace UmbCheckout.Testing;

/// <summary>One <see cref="EventMessages"/> per test, as Umbraco keeps one per request.</summary>
public sealed class TestEventMessagesFactory : IEventMessagesFactory
{
    public EventMessages Messages { get; } = new();

    public EventMessages Get() => Messages;

    public EventMessages? GetOrDefault() => Messages;
}
