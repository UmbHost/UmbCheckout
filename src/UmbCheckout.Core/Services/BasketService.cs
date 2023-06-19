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
    internal class BasketService : IBasketService
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
                if (lineItems.Any(x => x.Id.Equals(item.Id)))
                {
                    var lineItem = lineItems.First(x => x.Id.Equals(item.Id));
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
                    if (lineItems.Any(x => x.Id.Equals(item.Id)))
                    {
                        var lineItem = lineItems.First(x => x.Id.Equals(item.Id));
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
        public async Task<Basket> Reduce(Guid id)
        {
            try
            {
                var basket = await Get();

                using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                await _eventAggregator.PublishAsync(new OnBasketReduceStartedNotification(id, basket));

                var lineItems = basket.LineItems.ToList();
                if (lineItems.Any(x => x.Id.Equals(id)))
                {
                    var lineItem = lineItems.First(x => x.Id.Equals(id));
                    lineItem.Quantity--;
                    if (lineItem.Quantity == 0)
                    {
                        lineItems.Remove(lineItem);
                    }
                }

                basket.LineItems = lineItems;
                var updateResponse = await _sessionService.Update(basket);
                basket = updateResponse.Basket;

                scope.Notifications.Publish(new OnBasketReducedNotification(id, basket));
                return basket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Basket> Remove(Guid id)
        {
            try
            {
                var basket = await Get();

                using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                await _eventAggregator.PublishAsync(new OnBasketRemoveStartedNotification(id, basket));

                var lineItems = basket.LineItems.ToList();
                if (lineItems.Any(x => x.Id.Equals(id)))
                {
                    var lineItem = lineItems.First(x => x.Id.Equals(id));
                    lineItems.Remove(lineItem);
                }

                basket.LineItems = lineItems;
                var updateResponse = await _sessionService.Update(basket);
                basket = updateResponse.Basket;

                scope.Notifications.Publish(new OnBasketRemovedNotification(id, basket));
                return basket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Basket> Remove(IEnumerable<Guid> ids)
        {
            try
            {
                var basket = await Get();

                using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                await _eventAggregator.PublishAsync(new OnBasketRemoveManyStartedNotification(ids, basket));

                var lineItems = basket.LineItems.ToList();

                foreach (var id in ids)
                {
                    if (!lineItems.Any(x => x.Id.Equals(id)))
                        continue;

                    var lineItem = lineItems.First(x => x.Id.Equals(id));
                    lineItems.Remove(lineItem);
                }

                basket.LineItems = lineItems;
                var updateResponse = await _sessionService.Update(basket);
                basket = updateResponse.Basket;

                scope.Notifications.Publish(new OnBasketRemovedManyNotification(ids, basket));
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
                return basket.LineItems.Select(x => x.Price).Sum();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
