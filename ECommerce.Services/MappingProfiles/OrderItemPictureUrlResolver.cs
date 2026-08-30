using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Domain.Entities.OrderModule;
using ECommerce.Shared.DTOs.OrderDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Services.MappingProfiles
{
    internal class OrderItemPictureUrlResolver : IValueResolver<OrderItem, OrderItemDTO, string>
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderItemPictureUrlResolver(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public string Resolve(
            OrderItem source,
            OrderItemDTO destination,
            string destMember,
            ResolutionContext context
        )
        {
            if (string.IsNullOrEmpty(source.Product.PictureUrl))
                return string.Empty;

            if (
                source.Product.PictureUrl.StartsWith("http")
                || source.Product.PictureUrl.StartsWith("https")
            )
                return source.Product.PictureUrl;

            //Fall back to the host actually serving the request, so a deployment that
            //forgets to override URLs:BaseUrl does not hand clients localhost image links.
            var baseUrl = _configuration.GetSection("URLs")["BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                var request = _httpContextAccessor.HttpContext?.Request;
                baseUrl =
                    request is null
                        ? null
                        : $"{request.Scheme}://{request.Host}{request.PathBase}";
            }

            if (string.IsNullOrWhiteSpace(baseUrl))
                return string.Empty;

            return $"{baseUrl.TrimEnd('/')}/{source.Product.PictureUrl.TrimStart('/')}";
        }
    }
}
