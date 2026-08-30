using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Domain.Entities.ProductModule;
using ECommerce.Shared.DTOs.ProductDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using static System.Net.WebRequestMethods;

namespace ECommerce.Services.MappingProfiles
{
    public class ProductPictureUrlResolver : IValueResolver<Product, ProductDTO, string>
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductPictureUrlResolver(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public string Resolve(
            Product source,
            ProductDTO destination,
            string destMember,
            ResolutionContext context
        )
        {
            if (string.IsNullOrEmpty(source.PictureUrl))
                return string.Empty;

            if (source.PictureUrl.StartsWith("http") || source.PictureUrl.StartsWith("https"))
                return source.PictureUrl;

            //Fall back to the host actually serving the request. Without this, a deployment
            //that forgets to override URLs:BaseUrl silently hands clients localhost image links.
            var baseUrl = _configuration.GetSection("URLs")["BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
                baseUrl = BuildBaseUrlFromRequest();

            if (string.IsNullOrWhiteSpace(baseUrl))
                return string.Empty;

            return $"{baseUrl.TrimEnd('/')}/{source.PictureUrl.TrimStart('/')}";
        }

        private string? BuildBaseUrlFromRequest()
        {
            var request = _httpContextAccessor.HttpContext?.Request;

            return request is null ? null : $"{request.Scheme}://{request.Host}{request.PathBase}";
        }
    }
}
