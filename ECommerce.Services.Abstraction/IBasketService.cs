using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.Shared.DTOs.BasketDTOs;

namespace ECommerce.Services.Abstraction
{
    public interface IBasketService
    {
        Task<BasketDTO> CreateOrUpdateBasketAsync(BasketDTO CreateOrUpdatedBasket);

        Task<BasketDTO> GetBasketAsync(string basketId);

        Task<bool> DeleteBasketAsync(string basketId);
    }
}
