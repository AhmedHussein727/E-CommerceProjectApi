using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.Domain.Entities.OrderModule;

namespace ECommerce.Services.Specifications.OrderSpecifications
{
    public class OrderSpecification : BaseSpecifications<Order, Guid>
    {
        public OrderSpecification(string email)
            : base(O => O.UserEmail == email)
        {
            AddInclude(X => X.DeliveryMethod);
            AddInclude(X => X.Items);
            AddOrderByDescending(X => X.OrderDate);
        }

        public OrderSpecification(Guid Id, string email)
            : base(O => O.UserEmail == email && O.Id == Id)
        {
            AddInclude(O => O.DeliveryMethod);
            AddInclude(O => O.Items);
        }
    }
}
