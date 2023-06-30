using System.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Moq;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Services;
using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;
using Xunit;
using HttpContextAccessor = UmbCheckout.Tests.Helpers.HttpContextAccessor;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace UmbCheckout.Tests;

public class SessionServiceTests
{
    private readonly ISessionService _sessionService;

    private readonly Microsoft.AspNetCore.Http.HttpContextAccessor _httpContextAccessor =
        HttpContextAccessor.GetHttpContext("/testpage", "test.com");

    public SessionServiceTests()
    {
        var logger = new Mock<ILogger<SessionService>>();
        var eventAggregator = new Mock<IEventAggregator>();
        var coreScopeProvider = new Mock<ICoreScopeProvider>();
        var dataProtectionProvider = new EphemeralDataProtectionProvider();
        var databaseService = new Mock<IDatabaseService>();

        var mockScope = new Mock<IScope>();
        mockScope.Setup(x => x.Notifications).Returns(new Mock<IScopedNotificationPublisher>().Object);
        coreScopeProvider.Setup(x => x.CreateCoreScope(IsolationLevel.Unspecified, RepositoryCacheMode.Unspecified, null, null, null, false, true))
            .Returns(mockScope.Object);

        var mockConfiguration = new Mock<IConfigurationService>();
        var umbCheckoutConfiguration = new UmbCheckoutConfiguration
        {
            EnableShipping = true,
            Id = 1,
            CancelPageUrl = new List<MultiUrlPicker>
            {
                new()
                {
                    Icon = "icon-document", Name = "Checkout Cancelled", Url = "/checkout-cancelled/",
                    Udi = "umb://document/52ec9b32732240d4acf75d570e2753f0", Published = true, Trashed = false
                }
            },
            SuccessPageUrl = new List<MultiUrlPicker>
            {
                new()
                {
                    Icon = "icon-document", Name = "Checkout Success", Url = "/checkout-success/",
                    Udi = "umb://document/ce8d58448cca4ff7af0f6313f2b93cee", Published = true, Trashed = false
                }
            }
        };
        mockConfiguration.Setup(x => x.GetConfiguration().Result).Returns(umbCheckoutConfiguration);

        _sessionService = new SessionService(dataProtectionProvider, _httpContextAccessor, logger.Object, eventAggregator.Object, coreScopeProvider.Object, mockConfiguration.Object, databaseService.Object);
    }

    [Fact]
    public async void CreateSession()
    {
        // Arrange

        // Act
        var result = await _sessionService.Get();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Basket);
        Assert.False(string.IsNullOrEmpty(result.Basket.SessionId));
    }

    [Fact]
    public async void UpdateSession()
    {
        var session = await _sessionService.Get();
        
        Assert.NotNull(session);
        Assert.False(string.IsNullOrEmpty(session.Basket.SessionId));

        // Arrange
        var basket = new Basket
        {
            Id = session.Basket.Id,
            SessionId = session.Basket.SessionId,
            Total = session.Basket.Total,
            LineItems = new List<LineItem>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Quantity = 1,
                    Name = "Test Product",
                    Price = 10.00m
                }
            }
        };
        // Act
        var result = await _sessionService.Update(basket);

        // Assert
        Assert.NotNull(result);
        Assert.True(session.Basket.SessionId == basket.SessionId);
        Assert.True(result.Basket == basket);
    }
}