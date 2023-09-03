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
            Key = Guid.NewGuid(),
            Quantity = 1,
            Name = "Test Product",
            Price = 10.00m
        };

        // Act
        var result = await _basketService.Add(product);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result.LineItems, x => x.Key.Equals(product.Key));
    }

    [Fact]
    public async void IncreaseProductQuantityInBasket()
    {
        // Arrange
        var product = new LineItem
        {
            Key = Guid.NewGuid(),
            Quantity = 1,
            Name = "Test Product",
            Price = 10.00m
        };

        // Act
        var result = await _basketService.Add(product);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result.LineItems, x => x.Key.Equals(product.Key));

        var increasedQuantityResult = await _basketService.Add(product);

        Assert.NotNull(increasedQuantityResult);
        Assert.Contains(result.LineItems, x => x.Key.Equals(product.Key) && x.Quantity == 2);
    }

    [Fact]
    public async void ReduceProductQuantityInBasket()
    {
        // Arrange
        var product = new LineItem
        {
            Key = Guid.NewGuid(),
            Quantity = 2,
            Name = "Test Product",
            Price = 10.00m
        };

        // Act
        var result = await _basketService.Add(product);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result.LineItems, x => x.Key.Equals(product.Key));

        var reducedQuantityResult = await _basketService.Reduce(product.Key);

        Assert.NotNull(reducedQuantityResult);
        Assert.Contains(result.LineItems, x => x.Key.Equals(product.Key) && x.Quantity == 1);
    }

    [Fact]
    public async void RemoveProductFromBasket()
    {
        // Arrange
        var product = new LineItem
        {
            Key = Guid.NewGuid(),
            Quantity = 2,
            Name = "Test Product",
            Price = 10.00m
        };

        // Act
        var result = await _basketService.Add(product);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result.LineItems, x => x.Key.Equals(product.Key));

        var removedResult = await _basketService.Remove(product.Key);

        Assert.NotNull(removedResult);
        Assert.DoesNotContain(result.LineItems, x => x.Key.Equals(product.Key));
    }

    [Fact]
    public async void ClearBasket()
    {
        // Arrange
        var product = new LineItem
        {
            Key = Guid.NewGuid(),
            Quantity = 2,
            Name = "Test Product",
            Price = 10.00m
        };

        // Act
        var result = await _basketService.Add(product);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result.LineItems, x => x.Key.Equals(product.Key));

        var clearedBasket = await _basketService.Clear();
        Assert.NotNull(clearedBasket);
        Assert.False(clearedBasket.LineItems.Any());
    }

    [Fact]
    public async void TotalItemsInBasket()
    {
        // Arrange
        var product = new LineItem
        {
            Key = Guid.NewGuid(),
            Quantity = 2,
            Name = "Test Product",
            Price = 10.00m
        };

        // Act
        var result = await _basketService.Add(product);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result.LineItems, x => x.Key.Equals(product.Key));

        Assert.True(result.ItemCount == 1);
    }

    [Fact]
    public async void BasketSubtotal()
    {
        var products = new List<LineItem>
        {
            new()
            {
                Key = Guid.NewGuid(),
                Quantity = 2,
                Name = "Test Product",
                Price = 10.00m
            },
            new()
            {
                Key = Guid.NewGuid(),
                Quantity = 1,
                Name = "Test Product 2",
                Price = 20.00m
            }
        };

        // Act
        var result = await _basketService.Add(products);
        var subTotal = result.Total;
        // Assert
        Assert.NotNull(result);
        Assert.True(subTotal == 40.00m);
    }

    [Fact]
    public async void BasketServiceSubtotal()
    {
        var products = new List<LineItem>
        {
            new()
            {
                Key = Guid.NewGuid(),
                Quantity = 2,
                Name = "Test Product",
                Price = 10.00m
            },
            new()
            {
                Key = Guid.NewGuid(),
                Quantity = 1,
                Name = "Test Product 2",
                Price = 20.00m
            }
        };

        // Act
        var result = await _basketService.Add(products);
        var subTotal = await _basketService.SubTotal();
        // Assert
        Assert.NotNull(result);
        Assert.True(subTotal == 40.00m);
    }

    [Fact]
    public async void GetEmptyBasket()
    {
        // Act
        var result = await _basketService.Get();

        // Assert
        Assert.NotNull(result);
        Assert.False(result.LineItems.Any());
    }
}