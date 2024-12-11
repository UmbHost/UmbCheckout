using Microsoft.Extensions.Logging;
using UmbCheckout.Core.Interfaces;
using UmbCheckout.Core.Pocos;
using UmbCheckout.Shared.Models;
using Umbraco.Cms.Core.Mapping;

namespace UmbCheckout.Core.Services
{
    /// <summary>
    /// A service which handles the mapping the basket to and from the database
    /// </summary>
    public class DatabaseMapperService : IDatabaseMapperService
    {
        private readonly ILogger<DatabaseService> _logger;
        private readonly IUmbracoMapper _mapper;

        public DatabaseMapperService(ILogger<DatabaseService> logger, IUmbracoMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
        }

        /// <inheritdoc />
        public Basket? ToBasket(UmbCheckoutBasket umbCheckoutBasket)
        {
            var basket = _mapper.Map<UmbCheckoutBasket, Basket>(umbCheckoutBasket);
            return basket;
        }
    }
}
