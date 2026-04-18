using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Domain.Contracts;
using ECommerce.Domain.Entities.ProductModule;
using ECommerce.Services.Abstraction;
using ECommerce.Services.Exceptions;
using ECommerce.Services.Specifications.ProductSpecifications;
using ECommerce.Shared;
using ECommerce.Shared.CommonResponses;
using ECommerce.Shared.DTOs.ProductDTOs;

namespace ECommerce.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BrandDTO>> GetAllBrandsAsync()
        {
            var Brands = await _unitOfWork.GetRepository<ProductBrand, int>().GetAllAsync();

            return _mapper.Map<IEnumerable<BrandDTO>>(Brands);
        }

        public async Task<PaginatedResult<ProductDTO>> GetAllProductsAsync(
            ProductQueryParams queryParams
        )
        {
            var repo = _unitOfWork.GetRepository<Product, int>();
            //GetAllProductsWithBrandsAndTypes
            var spec = new ProductWithTypeAndBrandSpecification(queryParams);
            var products = await repo.GetAllAsync(spec);

            var productWithCountSpec = new ProductWithCountSpecifications(queryParams);
            var TotalCount = await repo.CountAsync(productWithCountSpec);
            var DataToReturn = _mapper.Map<IEnumerable<ProductDTO>>(products);

            var countOfReturnedData = DataToReturn.Count();

            return new PaginatedResult<ProductDTO>(
                queryParams.PageIndex,
                countOfReturnedData,
                TotalCount,
                DataToReturn
            );
        }

        public async Task<IEnumerable<TypeDTO>> GetAllTypesAsync()
        {
            var types = await _unitOfWork.GetRepository<ProductType, int>().GetAllAsync();

            return _mapper.Map<IEnumerable<TypeDTO>>(types);
        }

        public async Task<Result<ProductDTO>> GetProductByIdAsync(int id)
        {
            var spec = new ProductWithTypeAndBrandSpecification(id);
            var product = await _unitOfWork.GetRepository<Product, int>().GetByIdAsync(spec);

            if (product is null)
                return Error.NotFound(
                    $"Product.NotFound",
                    $"Product with this Id:{id} is not found"
                );

            return _mapper.Map<ProductDTO>(product);
        }
    }
}
