using Microsoft.Extensions.Logging;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared.Models;
using UmbCheckout.Shared.Notifications.Basket;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;

namespace UmbCheckout.Core.Services
{
    /// <summary>
    /// A service which handles all things around the basket
    /// </summary>
    public class BasketService : IBasketService
    {
        private readonly ISessionService _sessionService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICoreScopeProvider _coreScopeProvider;
        private readonly ILogger<BasketService> _logger;

        public BasketService(ISessionService sessionService, ILogger<BasketService> logger, IEventAggregator eventAggregator, ICoreScopeProvider coreScopeProvider)
        {
            _sessionService = sessionService;
            _logger = logger;
            _eventAggregator = eventAggregator;
            _coreScopeProvider = coreScopeProvider;
        }

        /// <inheritdoc />
        public async Task<Basket> Get()
        {
            try
            {
                var currentSession = await _sessionService.Get();
                return currentSession.Basket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Basket> Add(LineItem item)
        {
            try
            {
                var basket = await Get();

                using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                await _eventAggregator.PublishAsync(new OnBasketAddStartedNotification(item, basket));

                var lineItems = basket.LineItems.ToList();
                if (lineItems.Any(x => x.Key.Equals(item.Key)))
                {
                    var lineItem = lineItems.First(x => x.Key.Equals(item.Key));
                    lineItem.Quantity += item.Quantity;
                }
                else
                {
                    lineItems.Add(item);
                }

                basket.LineItems = lineItems;
                var updateResponse = await _sessionService.Update(basket);
                basket = updateResponse.Basket;
                
                scope.Notifications.Publish(new OnBasketAddedNotification(item, basket));
                return basket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Basket> Add(IEnumerable<LineItem> items)
        {
            try
            {
                using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                await _eventAggregator.PublishAsync(new OnBasketAddManyStartedNotification(items));

                var basket = await Get();
                var lineItems = basket.LineItems.ToList();

                foreach (var item in items)
                {
                    if (lineItems.Any(x => x.Key.Equals(item.Key)))
                    {
                        var lineItem = lineItems.First(x => x.Key.Equals(item.Key));
                        lineItem.Quantity++;
                    }
                    else
                    {
                        lineItems.Add(item);
                    }
                }

                basket.LineItems = lineItems;
                var updateResponse = await _sessionService.Update(basket);
                basket = updateResponse.Basket;

                scope.Notifications.Publish(new OnBasketAddedManyNotification(items, basket));
                return basket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Basket> Reduce(Guid key)
        {
            try
            {
                var basket = await Get();

                using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                await _eventAggregator.PublishAsync(new OnBasketReduceStartedNotification(key, basket));

                var lineItems = basket.LineItems.ToList();
                if (lineItems.Any(x => x.Key.Equals(key)))
                {
                    var lineItem = lineItems.First(x => x.Key.Equals(key));
                    lineItem.Quantity--;
                    if (lineItem.Quantity == 0)
                    {
                        lineItems.Remove(lineItem);
                    }
                }

                basket.LineItems = lineItems;
                var updateResponse = await _sessionService.Update(basket);
                basket = updateResponse.Basket;

                scope.Notifications.Publish(new OnBasketReducedNotification(key, basket));
                return basket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Basket> Remove(Guid key)
        {
            try
            {
                var basket = await Get();

                using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                await _eventAggregator.PublishAsync(new OnBasketRemoveStartedNotification(key, basket));

                var lineItems = basket.LineItems.ToList();
                if (lineItems.Any(x => x.Key.Equals(key)))
                {
                    var lineItem = lineItems.First(x => x.Key.Equals(key));
                    lineItems.Remove(lineItem);
                }

                basket.LineItems = lineItems;
                var updateResponse = await _sessionService.Update(basket);
                basket = updateResponse.Basket;

                scope.Notifications.Publish(new OnBasketRemovedNotification(key, basket));
                return basket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Basket> Remove(IEnumerable<Guid> keys)
        {
            try
            {
                var basket = await Get();

                using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                await _eventAggregator.PublishAsync(new OnBasketRemoveManyStartedNotification(keys, basket));

                var lineItems = basket.LineItems.ToList();

                foreach (var key in keys)
                {
                    if (!lineItems.Any(x => x.Key.Equals(key)))
                        continue;

                    var lineItem = lineItems.First(x => x.Key.Equals(key));
                    lineItems.Remove(lineItem);
                }

                basket.LineItems = lineItems;
                var updateResponse = await _sessionService.Update(basket);
                basket = updateResponse.Basket;

                scope.Notifications.Publish(new OnBasketRemovedManyNotification(keys, basket));
                return basket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Basket> Clear()
        {
            try
            {
                var basket = await Get();

                using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                await _eventAggregator.PublishAsync(new OnBasketClearStartedNotification(basket));

                var lineItems = Enumerable.Empty<LineItem>();
                basket.LineItems = lineItems;
                var updateResponse = await _sessionService.Update(basket);
                basket = updateResponse.Basket;

                scope.Notifications.Publish(new OnBasketClearedNotification(basket));
                return basket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<long> TotalItems()
        {
            try
            {
                var basket = await Get();
                return basket.LineItems.Select(x => x.Quantity).Sum();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<decimal> SubTotal()
        {
            try
            {
                var basket = await Get();

                return basket.LineItems.Sum(lineItem => lineItem.Price * lineItem.Quantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
