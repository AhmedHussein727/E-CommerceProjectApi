using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.Domain.Entities.BasketModule;

namespace ECommerce.Domain.Contracts
{
    public interface IBasketRepository
    {
        Task<CustomerBasket?> GetBasketAsync(string basketId);

        Task<CustomerBasket?> CreateOrUpdateBasketAsync(
            CustomerBasket basket,
            TimeSpan timeToLive = default
        ); //00:00:00

        Task<bool> DeleteBasketAsync(string basketId);
    }
}
