using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Domain.Entities.IdentityModule;
using ECommerce.Shared.DTOs.OrderDTOs;

namespace ECommerce.Services.MappingProfiles
{
    internal class AuthenticationProfile : Profile
    {
        public AuthenticationProfile()
        {
            CreateMap<Address, AddressDTO>().ReverseMap();
        }
    }
}
