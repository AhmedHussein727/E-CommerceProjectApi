using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Domain.Entities.IdentityModule;
using ECommerce.Services.Abstraction;
using ECommerce.Shared.CommonResponses;
using ECommerce.Shared.DTOs.IdentityDTOs;
using ECommerce.Shared.DTOs.OrderDTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IMapper mapper
        )
        {
            _userManager = userManager;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<bool> CheckEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            return user != null;
        }

        public async Task<Result<AddressDTO>> GetAddressAsync(string email)
        {
            var user = await _userManager
                .Users.Include(X => X.Address)
                .FirstOrDefaultAsync(U => U.Email == email);
            if (user is null)
                return Error.NotFound(
                    "User.NotFound",
                    $"User with this email:{email} was not found"
                );

            return _mapper.Map<AddressDTO>(user.Address);
        }

        public async Task<Result<UserDTO>> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
                return Error.NotFound(
                    "User.NotFound",
                    $"User with this email:{email} was not found"
                );

            return new UserDTO(user.Email!, user.DisplayName, await CreateTokenAsync(user));
        }

        public async Task<Result<UserDTO>> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);

            if (user is null)
                return Error.InvalidCredintals("User.InvalidCredintals");

            var IsPasswordValid = await _userManager.CheckPasswordAsync(user, loginDTO.Password);
            if (!IsPasswordValid)
                return Error.InvalidCredintals("User.InvalidCredintals");

            var token = await CreateTokenAsync(user);
            return new UserDTO(user.Email!, user.DisplayName, token);
        }

        public async Task<Result<UserDTO>> RegisterAsync(RegisterDTO registerDTO)
        {
            var user = new ApplicationUser()
            {
                Email = registerDTO.Email,
                DisplayName = registerDTO.DisplayName,
                PhoneNumber = registerDTO.PhoneNumber,
                UserName = registerDTO.UserName,
            };

            var identityResult = await _userManager.CreateAsync(user, registerDTO.Password);

            if (identityResult.Succeeded)
            {
                var token = await CreateTokenAsync(user);
                return new UserDTO(user.Email, user.DisplayName, token);
            }

            return identityResult
                .Errors.Select(E => Error.Validation(E.Code, E.Description))
                .ToList();
        }

        public async Task<Result<AddressDTO>> UpdateUserAddressAsync(
            string email,
            AddressDTO addressDTO
        )
        {
            var user = await _userManager
                .Users.Include(X => X.Address)
                .FirstOrDefaultAsync(U => U.Email == email);
            if (user is null)
                return Error.NotFound(
                    "User.NotFound",
                    $"User with this email:{email} was not found"
                );

            if (user.Address is not null) //Update
            {
                user.Address.FirstName = addressDTO.FirstName;
                user.Address.LastName = addressDTO.LastName;
                user.Address.City = addressDTO.City;
                user.Address.Street = addressDTO.Street;
                user.Address.Country = addressDTO.Country;
            }
            else //Create
            {
                user.Address = _mapper.Map<Address>(addressDTO);
            }

            await _userManager.UpdateAsync(user);

            return _mapper.Map<AddressDTO>(user.Address);
        }

        private async Task<string> CreateTokenAsync(ApplicationUser user)
        {
            //Token  [Issuer,Audience,claims,expires,signinCredintals]

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName!),
            };

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var secretKey = _configuration["JWTOptions:SecretKey"]!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWTOptions:Issuer"],
                audience: _configuration["JWTOptions:Audience"],
                expires: DateTime.UtcNow.AddHours(1),
                claims: claims,
                signingCredentials: cred
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
