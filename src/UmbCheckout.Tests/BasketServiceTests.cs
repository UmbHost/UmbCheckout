using System.Data;
using Microsoft.Extensions.Logging;
using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Services;
using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;
using Xunit;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace UmbCheckout.Tests;

public class BasketServiceTests
{
    private readonly IBasketService _basketService;

    public BasketServiceTests()
    {
        var logger = new Mock<ILogger<BasketService>>();
        var eventAggregator = new Mock<IEventAggregator>();
        var coreScopeProvider = new Mock<ICoreScopeProvider>();

        var sessionService = new Mock<ISessionService>();
        var checkoutSession = new UmbCheckoutSession();
        sessionService.Setup(x => x.Get()).ReturnsAsync(checkoutSession);
        sessionService.Setup(x => x.Update(It.IsAny<Basket>())).ReturnsAsync(checkoutSession);
        
        var mockScope = new Mock<IScope>();
        mockScope.Setup(x => x.Notifications).Returns(new Mock<IScopedNotificationPublisher>().Object);
        coreScopeProvider.Setup(x => x.CreateCoreScope(IsolationLevel.Unspecified, RepositoryCacheMode.Unspecified, null, null, null, false, true))
            .Returns(mockScope.Object);

        _basketService = new BasketService(sessionService.Object, logger.Object, eventAggregator.Object, coreScopeProvider.Object);
    }

    [Fact]
    public async void AddProductToBasket()
    {
        // Arrange
        var product = new LineItem
        {
            Id = Guid.NewGuid(),
            Quantity = 1,
            Name = "Test Product",
            Price = 10.00m
        };

        // Act
        var result = await _basketService.Add(product);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result.LineItems, x => x.Id.Equals(product.Id));
    }
}