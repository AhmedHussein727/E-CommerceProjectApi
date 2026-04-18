using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.Presentation.Attributes;
using ECommerce.Services.Abstraction;
using ECommerce.Shared;
using ECommerce.Shared.DTOs.ProductDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Presentation.Controllers
{
    public class ProductsController : ApiBaseController
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        //Get All Products
        [HttpGet]
        //GET:baseUrl/api/Products
        [RedisCache]
        public async Task<ActionResult<PaginatedResult<ProductDTO>>> GetAllProducts(
            [FromQuery] ProductQueryParams queryParams
        )
        {
            var products = await _productService.GetAllProductsAsync(queryParams);

            return Ok(products);
        }

        [HttpGet("{id}")]
        //GET:BaseUrl/api/Products/1
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);

            return HandleResult<ProductDTO>(result);
        }

        [HttpGet("brands")]
        //GET:BaseUrl/api/Products/brands
        public async Task<ActionResult<IEnumerable<BrandDTO>>> GetAllBrands()
        {
            var brands = await _productService.GetAllBrandsAsync();
            return Ok(brands);
        }

        //GET:BaseUrl/api/Products/types
        [HttpGet("types")]
        public async Task<ActionResult<IEnumerable<TypeDTO>>> GetAllTypes()
        {
            var types = await _productService.GetAllTypesAsync();
            return Ok(types);
        }
    }
}
