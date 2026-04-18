using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Domain.Contracts;
using ECommerce.Domain.Entities.BasketModule;
using ECommerce.Services.Abstraction;
using ECommerce.Services.Exceptions;
using ECommerce.Shared.DTOs.BasketDTOs;

namespace ECommerce.Services
{
    public class BasketService : IBasketService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;

        public BasketService(IBasketRepository basketRepository, IMapper mapper)
        {
            _basketRepository = basketRepository;
            _mapper = mapper;
        }

        public async Task<BasketDTO> CreateOrUpdateBasketAsync(BasketDTO CreateOrUpdatedBasket)
        {
            //1- Convert BasketDTO TO CustomerBasket
            var customerBasket = _mapper.Map<CustomerBasket>(CreateOrUpdatedBasket);

            var CreatedOrUpdatedBasket = await _basketRepository.CreateOrUpdateBasketAsync(
                customerBasket
            );

            return _mapper.Map<BasketDTO>(CreatedOrUpdatedBasket);
        }

        public async Task<bool> DeleteBasketAsync(string basketId) =>
            await _basketRepository.DeleteBasketAsync(basketId);

        public async Task<BasketDTO> GetBasketAsync(string basketId)
        {
            var basket = await _basketRepository.GetBasketAsync(basketId);

            if (basket is null)
                throw new BasketNotFoundException(basketId);

            return _mapper.Map<BasketDTO>(basket);
        }
    }
}
