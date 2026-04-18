using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.Shared.CommonResponses;
using ECommerce.Shared.DTOs.IdentityDTOs;
using ECommerce.Shared.DTOs.OrderDTOs;

namespace ECommerce.Services.Abstraction
{
    public interface IAuthenticationService
    {
        //Login
        //Email,Password=>Token,DisplayName,Email
        Task<Result<UserDTO>> LoginAsync(LoginDTO loginDTO);

        //Register
        //Email,Password,DisplayName,UserName,PhoneNumber=>Token,DisplayName,Email

        Task<Result<UserDTO>> RegisterAsync(RegisterDTO registerDTO);

        //CheckEmail
        //Email=>bool

        Task<bool> CheckEmailAsync(string email);

        //GetUserByEmail

        //Email=>UserDTO [Email-DisplayName-Token]
        //=> Not Found Error

        Task<Result<UserDTO>> GetUserByEmailAsync(string email);

        //GetUserAddress
        //Email=>AddressDTO
        Task<Result<AddressDTO>> GetAddressAsync(string email);

        //UpdateUserAddress
        //Email,AddressDTO=>AddressDTO
        Task<Result<AddressDTO>> UpdateUserAddressAsync(string email, AddressDTO addressDTO);
    }
}
