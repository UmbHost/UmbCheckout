using Microsoft.Extensions.Logging;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Shared;
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
        [Obsolete("Use Get(CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        public Task<Basket> Get() => Get(CancellationToken.None);

        /// <inheritdoc />
        public async Task<Basket> Get(CancellationToken cancellationToken)
        {
            var currentSession = await _sessionService.Get(cancellationToken);
            return currentSession.Basket;
        }

        /// <inheritdoc />
        [Obsolete("Use Add(LineItem, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        public Task<Basket> Add(LineItem item) => Add(item, CancellationToken.None);

        /// <inheritdoc />
        public async Task<Basket> Add(LineItem item, CancellationToken cancellationToken)
        {
            ValidateNewLine(item);

            // A basket holds one currency: the line goes to its own currency's basket.
            var currencyCode = item.CurrencyCode!.Trim().ToUpperInvariant();
            var basket = (await _sessionService.Get(currencyCode, cancellationToken)).Basket;

            using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
            await _eventAggregator.PublishAsync(new OnBasketAddStartedNotification(item, basket), cancellationToken);

            basket.LineItems = Merge(basket.LineItems, new[] { item });
            basket.CurrencyCode ??= currencyCode;
            var updateResponse = await _sessionService.Update(basket, cancellationToken);
            basket = updateResponse.Basket;

            scope.Notifications.Publish(new OnBasketAddedNotification(item, basket));
            return basket;
        }

        /// <inheritdoc />
        [Obsolete("Use Add(IEnumerable<LineItem>, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        public Task<Basket> Add(IEnumerable<LineItem> items) => Add(items, CancellationToken.None);

        /// <inheritdoc />
        public async Task<Basket> Add(IEnumerable<LineItem> items, CancellationToken cancellationToken)
        {
            var newLines = items.ToList();
            newLines.ForEach(ValidateNewLine);
            var currencies = newLines.Select(item => item.CurrencyCode!.Trim().ToUpperInvariant()).Distinct().ToList();
            if (currencies.Count != 1)
            {
                throw new ArgumentException("Items added together must share one currency (a basket belongs to one currency).", nameof(items));
            }

            using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
            await _eventAggregator.PublishAsync(new OnBasketAddManyStartedNotification(newLines), cancellationToken);

            var basket = (await _sessionService.Get(currencies[0], cancellationToken)).Basket;
            basket.LineItems = Merge(basket.LineItems, newLines);
            basket.CurrencyCode ??= currencies[0];
            var updateResponse = await _sessionService.Update(basket, cancellationToken);
            basket = updateResponse.Basket;

            scope.Notifications.Publish(new OnBasketAddedManyNotification(newLines, basket));
            return basket;
        }

        /// <summary>
        /// Adds each new line's quantity to the existing line for the same product, or appends it; the latest price wins.
        /// Throws, leaving the basket's lines unchanged, when a line would pass <see cref="Consts.MaxLineQuantity"/> or the
        /// basket <see cref="Consts.MaxLineItems"/> products.
        /// </summary>
        private static List<LineItem> Merge(IEnumerable<LineItem> existing, IEnumerable<LineItem> newLines)
        {
            var lineItems = existing.Select(Copy).ToList();
            foreach (var item in newLines)
            {
                var lineItem = lineItems.FirstOrDefault(x => x.Key.Equals(item.Key));
                if (lineItem == null)
                {
                    if (lineItems.Count >= Consts.MaxLineItems)
                    {
                        throw new ArgumentException($"A basket can hold at most {Consts.MaxLineItems} different products (the Stripe Checkout line item limit).", nameof(newLines));
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

        /// <summary>A copy of a stored line, so a refused add leaves the basket's own lines untouched.</summary>
        private static LineItem Copy(LineItem line) => new()
        {
            Key = line.Key,
            Name = line.Name,
            Description = line.Description,
            CurrencyCode = line.CurrencyCode,
            Price = line.Price,
            Quantity = line.Quantity,
            MetaData = line.MetaData
        };

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
        [Obsolete("Use Reduce(Guid, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        public Task<Basket> Reduce(Guid key) => Reduce(key, CancellationToken.None);

        /// <inheritdoc />
        public async Task<Basket> Reduce(Guid key, CancellationToken cancellationToken)
        {
            var basket = await Get(cancellationToken);

            using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
            await _eventAggregator.PublishAsync(new OnBasketReduceStartedNotification(key, basket), cancellationToken);

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
            var updateResponse = await _sessionService.Update(basket, cancellationToken);
            basket = updateResponse.Basket;

            scope.Notifications.Publish(new OnBasketReducedNotification(key, basket));
            return basket;
        }

        /// <inheritdoc />
        [Obsolete("Use Remove(Guid, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        public Task<Basket> Remove(Guid key) => Remove(key, CancellationToken.None);

        /// <inheritdoc />
        public async Task<Basket> Remove(Guid key, CancellationToken cancellationToken)
        {
            var basket = await Get(cancellationToken);

            using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
            await _eventAggregator.PublishAsync(new OnBasketRemoveStartedNotification(key, basket), cancellationToken);

            var lineItems = basket.LineItems.ToList();
            if (lineItems.Any(x => x.Key.Equals(key)))
            {
                var lineItem = lineItems.First(x => x.Key.Equals(key));
                lineItems.Remove(lineItem);
            }

            basket.LineItems = lineItems;
            var updateResponse = await _sessionService.Update(basket, cancellationToken);
            basket = updateResponse.Basket;

            scope.Notifications.Publish(new OnBasketRemovedNotification(key, basket));
            return basket;
        }

        /// <inheritdoc />
        [Obsolete("Use Remove(IEnumerable<Guid>, CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        public Task<Basket> Remove(IEnumerable<Guid> keys) => Remove(keys, CancellationToken.None);

        /// <inheritdoc />
        public async Task<Basket> Remove(IEnumerable<Guid> keys, CancellationToken cancellationToken)
        {
            var basket = await Get(cancellationToken);

            using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
            await _eventAggregator.PublishAsync(new OnBasketRemoveManyStartedNotification(keys, basket), cancellationToken);

            var lineItems = basket.LineItems.ToList();

            foreach (var key in keys)
            {
                if (!lineItems.Any(x => x.Key.Equals(key)))
                    continue;

                var lineItem = lineItems.First(x => x.Key.Equals(key));
                lineItems.Remove(lineItem);
            }

            basket.LineItems = lineItems;
            var updateResponse = await _sessionService.Update(basket, cancellationToken);
            basket = updateResponse.Basket;

            scope.Notifications.Publish(new OnBasketRemovedManyNotification(keys, basket));
            return basket;
        }

        /// <inheritdoc />
        [Obsolete("Use Clear(CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        public Task<Basket> Clear() => Clear(CancellationToken.None);

        /// <inheritdoc />
        public async Task<Basket> Clear(CancellationToken cancellationToken)
        {
            var basket = await Get(cancellationToken);

            using var scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
            await _eventAggregator.PublishAsync(new OnBasketClearStartedNotification(basket), cancellationToken);

            var lineItems = Enumerable.Empty<LineItem>();
            basket.LineItems = lineItems;
            var updateResponse = await _sessionService.Update(basket, cancellationToken);
            basket = updateResponse.Basket;

            scope.Notifications.Publish(new OnBasketClearedNotification(basket));
            return basket;
        }

        /// <inheritdoc />
        [Obsolete("Use TotalItems(CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        public Task<long> TotalItems() => TotalItems(CancellationToken.None);

        /// <inheritdoc />
        public async Task<long> TotalItems(CancellationToken cancellationToken)
        {
            var basket = await Get(cancellationToken);
            return basket.LineItems.Select(x => x.Quantity).Sum();
        }

        /// <inheritdoc />
        [Obsolete("Use SubTotal(CancellationToken) (the overload with a CancellationToken). Scheduled for removal in 17.1.")]
        public Task<decimal> SubTotal() => SubTotal(CancellationToken.None);

        /// <inheritdoc />
        public async Task<decimal> SubTotal(CancellationToken cancellationToken)
        {
            var basket = await Get(cancellationToken);

            return basket.LineItems.Sum(lineItem => lineItem.Price * lineItem.Quantity);
        }
    }
}
