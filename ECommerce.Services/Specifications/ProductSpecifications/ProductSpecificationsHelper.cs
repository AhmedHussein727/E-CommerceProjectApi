using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ECommerce.Domain.Entities.ProductModule;
using ECommerce.Shared;

namespace ECommerce.Services.Specifications.ProductSpecifications
{
    internal static class ProductSpecificationsHelper
    {
        public static Expression<Func<Product, bool>> GetCriteria(ProductQueryParams queryParams)
        {
            return P =>
                (!queryParams.brandId.HasValue || P.ProductBrandId == queryParams.brandId.Value)
                && (!queryParams.typeId.HasValue || P.ProductTypeId == queryParams.typeId.Value)
                && (
                    string.IsNullOrEmpty(queryParams.search)
                    || P.Name.ToLower().Contains(queryParams.search.ToLower())
                );
        }
    }
}
