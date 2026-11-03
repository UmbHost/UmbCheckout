using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StripeClient = Stripe.StripeClient;
using SystemNetHttpClient = Stripe.SystemNetHttpClient;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Models;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Stripe.Notifications;
using UmbCheckout.Stripe.Services;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Xunit;

namespace UmbCheckout.Tests.Services;

/// <summary>Session-option handlers run before the session is created, and what they change reaches Stripe.</summary>
public sealed class StripeSessionServiceHookTests
{
    private readonly List<string> _order = [];
    private readonly StubStripe _stripe;
    private readonly Mock<IEventAggregator> _events = new();
    private readonly Guid _key = Guid.NewGuid();

    public StripeSessionServiceHookTests() => _stripe = new StubStripe(_order);

    /// <summary>Answers every Stripe call with a checkout session and records the (unescaped) request body.</summary>
    private sealed class StubStripe(List<string> order) : HttpMessageHandler
    {
        public List<string> Bodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            order.Add("stripe");
            Bodies.Add(Uri.UnescapeDataString(await request.Content.Required().ReadAsStringAsync(cancellationToken)));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":\"cs_test_1\",\"object\":\"checkout.session\",\"livemode\":false,\"url\":\"https://checkout.stripe.com/c/pay/cs_test_1\"}",
                    Encoding.UTF8, "application/json")
            };
        }
    }

    private StripeSessionService Service()
    {
        var product = new Mock<IPublishedContent> { DefaultValue = DefaultValue.Mock };
        product.Setup(p => p.Key).Returns(_key);
        product.Setup(p => p.Name).Returns("Ping Pong Ball");
        var contentCache = new Mock<IPublishedContentCache>();
        contentCache.Setup(c => c.GetById(_key)).Returns(product.Object);

        var factory = new Mock<IUmbCheckoutStripeClientFactory>();
        factory.Setup(f => f.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new StripeClient("sk_test_123", httpClient: new SystemNetHttpClient(new HttpClient(_stripe))));
        var stripeSettings = new Mock<IStripeSettingsService>();
        stripeSettings.Setup(s => s.GetStripeSettingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync((UmbCheckout.Stripe.Models.UmbCheckoutStripeSettings?)null);

        return new StripeSessionService(contentCache.Object, new Mock<ICoreScopeProvider> { DefaultValue = DefaultValue.Mock }.Object, _events.Object,
            NullLogger<StripeSessionService>.Instance, Mock.Of<IConfigurationService>(), Mock.Of<IHttpContextAccessor>(), Mock.Of<IStripeShippingOptionsService>(),
            UmbCheckout.Testing.TestLicenseService.None, stripeSettings.Object, factory.Object);
    }

    private Basket Basket() => new()
    {
        SiteRootCulture = "en-GB",
        CurrencyCode = "GBP",
        LineItems = [new LineItem { Key = _key, Name = "Ping Pong Ball", Description = "Table tennis ball", Price = 2m, CurrencyCode = "GBP", Quantity = 1 }]
    };

    private void On<TNotification>(Action<TNotification> handler) where TNotification : INotification
        => _events.Setup(e => e.PublishAsync(It.IsAny<TNotification>(), It.IsAny<CancellationToken>()))
            .Callback<TNotification, CancellationToken>((notification, _) => handler(notification))
            .Returns(Task.CompletedTask);

    [Fact]
    public async Task What_handlers_change_reaches_Stripe()
    {
        // The notifications were queued until the scope ended, after the session had been sent: changes were ignored.
        On<OnProviderSessionOptionsLineItemAdding>(n => { _order.Add("line adding"); n.LineItem.PriceData.UnitAmount = 150; });
        On<OnProviderSessionOptionsStarted>(n => { _order.Add("started"); n.Options.Metadata["site_order"] = "42"; });
        On<OnProviderSessionOptionsCreated>(n => { _order.Add("created"); n.Options.ClientReferenceId = "order-42"; });

        await Service().CreateSessionAsync(Basket(), CancellationToken.None);

        var body = Assert.Single(_stripe.Bodies);
        Assert.Contains("line_items[0][price_data][unit_amount]=150", body);
        Assert.Contains("metadata[site_order]=42", body);
        Assert.Contains("client_reference_id=order-42", body);
        Assert.Equal(["line adding", "started", "created", "stripe"], _order);
    }

    [Fact]
    public async Task A_throwing_handler_stops_checkout_before_Stripe()
    {
        _events.Setup(e => e.PublishAsync(It.IsAny<OnProviderSessionOptionsCreated>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("a site rule refused this basket"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => Service().CreateSessionAsync(Basket(), CancellationToken.None));

        Assert.Empty(_stripe.Bodies);
    }
}
