using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.Shared;
using ECommerce.Shared.CommonResponses;
using ECommerce.Shared.DTOs.ProductDTOs;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ECommerce.Services.Abstraction
{
    public interface IProductService
    {
        // Get All Products Return IEnumerable Of Products Data Which Will be {Id , Name, Description , PictureUrl , Price , ProductBrand, ProductType}

        Task<PaginatedResult<ProductDTO>> GetAllProductsAsync(ProductQueryParams queryParams);

        //Get Product By Id Return Product Data Which Will be { Id , Name, Description , PictureUrl , Price , ProductBrand, ProductType }

        Task<Result<ProductDTO>> GetProductByIdAsync(int id);

        Task<IEnumerable<BrandDTO>> GetAllBrandsAsync();

        Task<IEnumerable<TypeDTO>> GetAllTypesAsync();
    }
}
