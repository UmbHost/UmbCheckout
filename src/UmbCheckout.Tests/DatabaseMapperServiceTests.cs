using Microsoft.Extensions.Logging;
using Moq;
using UmbCheckout.Core.Pocos;
using UmbCheckout.Core.Services;
using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Mapping;
using Xunit;

namespace UmbCheckout.Tests
{
    public class DatabaseMapperServiceTests
    {
        [Fact]
        public void ToBasket()
        {
            // Arrange
            var basket = new Basket
            {
                SessionId = "123",
                LineItems = new List<LineItem>
                {
                    new LineItem
                    {
                        Key = Guid.NewGuid(),
                        Quantity = 1,
                        Name = "Test Product",
                        Price = 10.00m
                    }
                },
                MetaData = new Dictionary<string, string> { { "Test Meta Data 1 Key", "Test Meta Data 1 Value" } }
            };

            var umbCheckoutBasket = new UmbCheckoutBasket
            {
                Id = 1,
                SessionId = "123",
                LineItems = "[{\"Key\":\"69f5e672-d438-4023-a1b8-cc33f89c551f\",\"Name\":\"Test Product\",\"Description\":\"\",\"CurrencyCode\":\"\",\"Price\":10.00,\"CurrencyPrice\":\"10.00\",\"Quantity\":1,\"MetaData\":{}}]",
                MetaData = "{\"Test Meta Data 1 Key\":\"Test Meta Data 1 Value\"}",
                BasketTotal = 10.00m
            };

            var logger = new Mock<ILogger<DatabaseService>>();

            var mapper = new Mock<IUmbracoMapper>();
            mapper.Setup(m => m.Map<UmbCheckoutBasket, Basket>(It.IsAny<UmbCheckoutBasket>()))
                .Returns(basket);

            var databaseMapperService = new DatabaseMapperService(logger.Object, mapper.Object);
            // Act
            var result = databaseMapperService.ToBasket(umbCheckoutBasket);
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Total == 10.00m);
            Assert.Equal(basket.LineItems.Count(), result.LineItems.Count());
        }
    }
}
