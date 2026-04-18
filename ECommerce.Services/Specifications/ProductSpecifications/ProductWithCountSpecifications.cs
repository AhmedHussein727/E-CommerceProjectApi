using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.Domain.Entities.ProductModule;
using ECommerce.Shared;

namespace ECommerce.Services.Specifications.ProductSpecifications
{
    internal class ProductWithCountSpecifications : BaseSpecifications<Product, int>
    {
        public ProductWithCountSpecifications(ProductQueryParams queryParams)
            : base(ProductSpecificationsHelper.GetCriteria(queryParams)) { }
    }
}
