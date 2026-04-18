using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ECommerce.Services.Abstraction;
using ECommerce.Shared.DTOs.IdentityDTOs;
using ECommerce.Shared.DTOs.OrderDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Presentation.Controllers
{
    public class AuthenticationController : ApiBaseController
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        //Login
        //POST:BaseUrl/Api/Authentication/Login
        [HttpPost("Login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            var result = await _authenticationService.LoginAsync(loginDTO);

            return HandleResult(result);
        }

        //Register
        //POST:baseUrl/api/Authentication/Register

        [HttpPost("Register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            var result = await _authenticationService.RegisterAsync(registerDTO);

            return HandleResult(result);
        }

        [HttpGet("emailExists")]
        public async Task<ActionResult<bool>> CheckEmail(string email)
        {
            var Result = await _authenticationService.CheckEmailAsync(email);

            return Ok(Result);
        }

        [Authorize]
        [HttpGet("CurrentUser")]
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            var email = GetEmailFromToken();

            var Result = await _authenticationService.GetUserByEmailAsync(email!);

            return HandleResult(Result);
        }

        [ProducesResponseType<AddressDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [Authorize]
        [HttpGet("Address")]
        //baseUsr/api/Authentication/Address

        public async Task<ActionResult<AddressDTO>> GetAddress()
        {
            var email = GetEmailFromToken();
            var result = await _authenticationService.GetAddressAsync(email);
            return HandleResult(result);
        }

        [Authorize]
        [HttpPut("Address")]
        //baseUrl/api/Authentication/Address
        public async Task<ActionResult<AddressDTO>> UpdateAddress(AddressDTO addressDTO)
        {
            var email = GetEmailFromToken();
            var result = await _authenticationService.UpdateUserAddressAsync(email, addressDTO);
            return HandleResult(result);
        }
    }
}
