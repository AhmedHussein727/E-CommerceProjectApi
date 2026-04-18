using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ECommerce.Domain.Contracts;
using ECommerce.Domain.Entities.BasketModule;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace ECommerce.Persistence.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IDatabase _database;

        public BasketRepository(IConnectionMultiplexer connection)
        {
            _database = connection.GetDatabase();
        }

        public async Task<CustomerBasket?> CreateOrUpdateBasketAsync(
            CustomerBasket basket,
            TimeSpan timeToLive = default
        )
        {
            var jsonBasket = JsonSerializer.Serialize(basket);
            var isCreatedOrUpdated = await _database.StringSetAsync(
                basket.Id,
                jsonBasket,
                (timeToLive == default) ? TimeSpan.FromDays(7) : timeToLive
            );

            return await GetBasketAsync(basket.Id);
        }

        public async Task<bool> DeleteBasketAsync(string basketId) =>
            await _database.KeyDeleteAsync(basketId);

        public async Task<CustomerBasket?> GetBasketAsync(string basketId)
        {
            var basket = await _database.StringGetAsync(basketId);

            if (basket.IsNullOrEmpty)
                return null;
            else
                return JsonSerializer.Deserialize<CustomerBasket>(basket!);
        }
    }
}
