using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Domain.Entities.BasketModule;
using ECommerce.Shared.DTOs.BasketDTOs;

namespace ECommerce.Services.MappingProfiles
{
    internal class BasketProfile : Profile
    {
        public BasketProfile()
        {
            CreateMap<BasketDTO, CustomerBasket>().ReverseMap();

            CreateMap<BasketItemDTO, BasketItem>().ReverseMap();
        }
    }
}
