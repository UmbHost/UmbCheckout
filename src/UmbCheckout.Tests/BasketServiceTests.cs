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
    private readonly Mock<ISessionService> _sessionService = new();

    public BasketServiceTests()
    {
        var logger = new Mock<ILogger<BasketService>>();
        var eventAggregator = new Mock<IEventAggregator>();
        var coreScopeProvider = new Mock<ICoreScopeProvider>();

        var checkoutSession = new UmbCheckoutSession();
        _sessionService.Setup(x => x.Get()).ReturnsAsync(checkoutSession);
        _sessionService.Setup(x => x.Get(It.IsAny<string?>())).ReturnsAsync(checkoutSession);
        _sessionService.Setup(x => x.Update(It.IsAny<Basket>())).ReturnsAsync(checkoutSession);

        var mockScope = new Mock<IScope>();
        mockScope.Setup(x => x.Notifications).Returns(new Mock<IScopedNotificationPublisher>().Object);
        coreScopeProvider.Setup(x => x.CreateCoreScope(IsolationLevel.Unspecified, RepositoryCacheMode.Unspecified, null, null, null, false, true))
            .Returns(mockScope.Object);

        _basketService = new BasketService(_sessionService.Object, logger.Object, eventAggregator.Object, coreScopeProvider.Object);
    }

    [Fact]
    public async Task AddProductToBasket()
    {
        // Arrange
        var product = new LineItem
        {
            Key = Guid.NewGuid(),
            Quantity = 1,
            Name = "Test Product",
            Price = 10.00m,
            CurrencyCode = "GBP"
        };

        // Act
        var result = await _basketService.Add(product);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result.LineItems, x => x.Key.Equals(product.Key));
    }

    [Fact]
    public async Task IncreaseProductQuantityInBasket()
    {
        // Arrange
        var product = new LineItem
        {
            Key = Guid.NewGuid(),
            Quantity = 1,
            Name = "Test Product",
            Price = 10.00m,
            CurrencyCode = "GBP"
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
    public async Task ReduceProductQuantityInBasket()
    {
        // Arrange
        var product = new LineItem
        {
            Key = Guid.NewGuid(),
            Quantity = 2,
            Name = "Test Product",
            Price = 10.00m,
            CurrencyCode = "GBP"
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
    public async Task RemoveProductFromBasket()
    {
        // Arrange
        var product = new LineItem
        {
            Key = Guid.NewGuid(),
            Quantity = 2,
            Name = "Test Product",
            Price = 10.00m,
            CurrencyCode = "GBP"
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
    public async Task ClearBasket()
    {
        // Arrange
        var product = new LineItem
        {
            Key = Guid.NewGuid(),
            Quantity = 2,
            Name = "Test Product",
            Price = 10.00m,
            CurrencyCode = "GBP"
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
    public async Task TotalItemsInBasket()
    {
        // Arrange
        var product = new LineItem
        {
            Key = Guid.NewGuid(),
            Quantity = 2,
            Name = "Test Product",
            Price = 10.00m,
            CurrencyCode = "GBP"
        };

        // Act
        var result = await _basketService.Add(product);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result.LineItems, x => x.Key.Equals(product.Key));

        Assert.True(result.ItemCount == 1);
    }

    [Fact]
    public async Task BasketSubtotal()
    {
        var products = new List<LineItem>
        {
            new()
            {
                Key = Guid.NewGuid(),
                Quantity = 2,
                Name = "Test Product",
                Price = 10.00m,
                CurrencyCode = "GBP"
            },
            new()
            {
                Key = Guid.NewGuid(),
                Quantity = 1,
                Name = "Test Product 2",
                Price = 20.00m,
                CurrencyCode = "GBP"
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
    public async Task BasketServiceSubtotal()
    {
        var products = new List<LineItem>
        {
            new()
            {
                Key = Guid.NewGuid(),
                Quantity = 2,
                Name = "Test Product",
                Price = 10.00m,
                CurrencyCode = "GBP"
            },
            new()
            {
                Key = Guid.NewGuid(),
                Quantity = 1,
                Name = "Test Product 2",
                Price = 20.00m,
                CurrencyCode = "GBP"
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
    public async Task GetEmptyBasket()
    {
        // Act
        var result = await _basketService.Get();

        // Assert
        Assert.NotNull(result);
        Assert.False(result.LineItems.Any());
    }

    private static LineItem Gbp(long quantity = 1, Guid? key = null) => new() { Key = key ?? Guid.NewGuid(), Name = "Test Product", Price = 10.00m, CurrencyCode = "GBP", Quantity = quantity };

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1_000_000)]
    public async Task Add_RefusesAQuantityOutsideOneToTheCap(long quantity)
        => await Assert.ThrowsAnyAsync<ArgumentException>(() => _basketService.Add(Gbp(quantity)));

    [Fact]
    public async Task Add_RefusesWhenTheMergedQuantityPassesTheCap()
    {
        var key = Guid.NewGuid();
        await _basketService.Add(Gbp(999_990, key));

        await Assert.ThrowsAnyAsync<ArgumentException>(() => _basketService.Add(Gbp(10, key)));
    }

    [Fact]
    public async Task Add_RefusesAHundredAndFirstProduct()
    {
        // Stripe Checkout takes at most 100 line items; the 101st must be refused here, not at checkout.
        (await _basketService.Get()).LineItems = Enumerable.Range(0, 100).Select(_ => Gbp()).ToList();

        await Assert.ThrowsAnyAsync<ArgumentException>(() => _basketService.Add(Gbp()));
    }

    [Fact]
    public async Task Add_MoreOfAProductAlreadyInAFullBasketIsAllowed()
    {
        var key = Guid.NewGuid();
        (await _basketService.Get()).LineItems = Enumerable.Range(0, 99).Select(_ => Gbp()).Append(Gbp(1, key)).ToList();

        var basket = await _basketService.Add(Gbp(1, key));

        Assert.Equal(2, basket.LineItems.Single(l => l.Key == key).Quantity);
    }

    [Fact]
    public async Task Add_RefusesALineWithoutACurrency()
        => await Assert.ThrowsAnyAsync<ArgumentException>(() => _basketService.Add(new LineItem { Key = Guid.NewGuid(), Quantity = 1, Price = 1m }));

    [Fact]
    public async Task Add_GoesToTheLinesOwnCurrencyBasket()
    {
        await _basketService.Add(new LineItem { Key = Guid.NewGuid(), Quantity = 1, Price = 1m, CurrencyCode = "USD" });

        _sessionService.Verify(s => s.Get("USD"), Times.Once);
    }

    [Fact]
    public async Task AddMany_AddsTheRequestedQuantities()
    {
        var key = Guid.NewGuid();
        await _basketService.Add([Gbp(2, key)]);
        var basket = await _basketService.Add([Gbp(3, key)]);

        Assert.Equal(5, Assert.Single(basket.LineItems, l => l.Key == key).Quantity);
    }

    [Fact]
    public async Task AddMany_RefusesMixedCurrencies()
        => await Assert.ThrowsAnyAsync<ArgumentException>(() => _basketService.Add([Gbp(), new LineItem { Key = Guid.NewGuid(), Quantity = 1, CurrencyCode = "USD" }]));

    [Fact]
    public async Task Reduce_RemovesALineAtOrBelowZero()
    {
        // A basket saved before quantities were validated can hold a 0 line; reducing it must remove it, not go negative.
        var key = Guid.NewGuid();
        (await _basketService.Get()).LineItems = [Gbp(0, key)];

        var basket = await _basketService.Reduce(key);

        Assert.DoesNotContain(basket.LineItems, l => l.Key == key);
    }
}
