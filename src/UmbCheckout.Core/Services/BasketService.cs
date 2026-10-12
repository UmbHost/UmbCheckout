using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared;
using UmbCheckout.Shared.Models;
using UmbCheckout.Shared.Notifications;
using UmbCheckout.Shared.Notifications.Basket;
using Umbraco.Cms.Core.DependencyInjection;
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
        private readonly IEventMessagesFactory _eventMessagesFactory;

        public BasketService(ISessionService sessionService, ILogger<BasketService> logger, IEventAggregator eventAggregator, ICoreScopeProvider coreScopeProvider,
            IEventMessagesFactory eventMessagesFactory)
        {
            _sessionService = sessionService;
            _logger = logger;
            _eventAggregator = eventAggregator;
            _coreScopeProvider = coreScopeProvider;
            _eventMessagesFactory = eventMessagesFactory;
        }

        [Obsolete("Use the constructor that takes an IEventMessagesFactory. Scheduled for removal in the next major version.")]
        public BasketService(ISessionService sessionService, ILogger<BasketService> logger, IEventAggregator eventAggregator, ICoreScopeProvider coreScopeProvider)
            : this(sessionService, logger, eventAggregator, coreScopeProvider, StaticServiceProvider.Instance.GetRequiredService<IEventMessagesFactory>())
        {
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
            ValidateNewLine(item);

            // A basket holds one currency: the line goes to its own currency's basket.
            var basket = await Stored(item.CurrencyCode);

            using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
            await _eventAggregator.PublishAsync(new OnBasketAddStartedNotification(item, basket));

            basket.LineItems = Merge(basket.LineItems, [item]);
            basket.CurrencyCode ??= item.CurrencyCode;
            (basket, var saved) = await Store(basket);

            if (saved)
            {
                scope.Notifications.Publish(new OnBasketAddedNotification(item, basket));
            }

            return basket;
        }

        /// <inheritdoc />
        public async Task<Basket> Add(IEnumerable<LineItem> items)
        {
            var newLines = items.ToList();
            newLines.ForEach(ValidateNewLine);
            var currencies = newLines.Select(item => item.CurrencyCode!.Trim().ToUpperInvariant()).Distinct().ToList();
            if (currencies.Count != 1)
            {
                throw new ArgumentException("Items added together must share one currency (a basket belongs to one currency).", nameof(items));
            }

            using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
            await _eventAggregator.PublishAsync(new OnBasketAddManyStartedNotification(newLines));

            var basket = await Stored(currencies[0]);
            basket.LineItems = Merge(basket.LineItems, newLines);
            basket.CurrencyCode ??= currencies[0];
            (basket, var saved) = await Store(basket);

            if (saved)
            {
                scope.Notifications.Publish(new OnBasketAddedManyNotification(newLines, basket));
            }

            return basket;
        }

        /// <summary>The stored basket, without the Loaded hooks: a change starts from what is stored, never from what a site displays.</summary>
        private async Task<Basket> Stored(string? currencyCode = null)
        {
            using (UmbCheckoutHooks.SuppressLoaded())
            {
                return currencyCode == null ? (await _sessionService.Get()).Basket : (await _sessionService.Get(currencyCode)).Basket;
            }
        }

        /// <summary>Stores the changed basket; Saved is false when a site's handler cancelled the change (the stored basket comes back unchanged).</summary>
        private async Task<(Basket Basket, bool Saved)> Store(Basket basket)
        {
            var messages = _eventMessagesFactory.Get();
            var cancelled = messages.CancellationCount();
            var stored = (await _sessionService.Update(basket)).Basket;
            return (stored, messages.CancellationCount() == cancelled);
        }

        /// <summary>Adds each new line's quantity to an existing line for the same product, or appends it; the latest price wins.</summary>
        private static List<LineItem> Merge(IEnumerable<LineItem> existing, IEnumerable<LineItem> newLines)
        {
            var lineItems = existing.ToList();
            foreach (var item in newLines)
            {
                var lineItem = lineItems.FirstOrDefault(x => x.Key.Equals(item.Key));
                if (lineItem == null)
                {
                    if (lineItems.Count >= Consts.MaxLineItems)
                    {
                        throw new ArgumentException($"A basket can hold at most {Consts.MaxLineItems} different products (Stripe Checkout's line item limit).", nameof(newLines));
                    }

                    lineItems.Add(item);
                    continue;
                }

                if (lineItem.Quantity + item.Quantity > Consts.MaxLineQuantity)
                {
                    throw new ArgumentOutOfRangeException(nameof(newLines), lineItem.Quantity + item.Quantity, $"A basket line can hold at most {Consts.MaxLineQuantity}.");
                }

                lineItem.Quantity += item.Quantity;
                lineItem.Price = item.Price;
            }

            return lineItems;
        }

        private static void ValidateNewLine(LineItem item)
        {
            if (string.IsNullOrWhiteSpace(item.CurrencyCode))
            {
                throw new ArgumentException("A basket line needs a currency.", nameof(item));
            }

            if (item.Quantity < 1 || item.Quantity > Consts.MaxLineQuantity)
            {
                throw new ArgumentOutOfRangeException(nameof(item), item.Quantity, $"Quantity must be between 1 and {Consts.MaxLineQuantity}.");
            }
        }

        /// <inheritdoc />
        public async Task<Basket> Reduce(Guid key)
        {
            try
            {
                var basket = await Stored();

                using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                await _eventAggregator.PublishAsync(new OnBasketReduceStartedNotification(key, basket));

                var lineItems = basket.LineItems.ToList();
                if (lineItems.Any(x => x.Key.Equals(key)))
                {
                    var lineItem = lineItems.First(x => x.Key.Equals(key));
                    lineItem.Quantity--;
                    if (lineItem.Quantity <= 0)
                    {
                        lineItems.Remove(lineItem);
                    }
                }

                basket.LineItems = lineItems;
                (basket, var saved) = await Store(basket);

                if (saved)
                {
                    scope.Notifications.Publish(new OnBasketReducedNotification(key, basket));
                }

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
                var basket = await Stored();

                using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                await _eventAggregator.PublishAsync(new OnBasketRemoveStartedNotification(key, basket));

                var lineItems = basket.LineItems.ToList();
                if (lineItems.Any(x => x.Key.Equals(key)))
                {
                    var lineItem = lineItems.First(x => x.Key.Equals(key));
                    lineItems.Remove(lineItem);
                }

                basket.LineItems = lineItems;
                (basket, var saved) = await Store(basket);

                if (saved)
                {
                    scope.Notifications.Publish(new OnBasketRemovedNotification(key, basket));
                }

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
                var basket = await Stored();

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
                (basket, var saved) = await Store(basket);

                if (saved)
                {
                    scope.Notifications.Publish(new OnBasketRemovedManyNotification(keys, basket));
                }

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
                var basket = await Stored();

                using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
                await _eventAggregator.PublishAsync(new OnBasketClearStartedNotification(basket));

                var lineItems = Enumerable.Empty<LineItem>();
                basket.LineItems = lineItems;
                (basket, var saved) = await Store(basket);

                if (saved)
                {
                    scope.Notifications.Publish(new OnBasketClearedNotification(basket));
                }

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
