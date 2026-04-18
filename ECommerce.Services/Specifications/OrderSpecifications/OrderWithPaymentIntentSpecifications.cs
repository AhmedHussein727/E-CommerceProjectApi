using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.Domain.Entities.OrderModule;

namespace ECommerce.Services.Specifications.OrderSpecifications
{
    internal class OrderWithPaymentIntentSpecifications : BaseSpecifications<Order, Guid>
    {
        public OrderWithPaymentIntentSpecifications(string paymentIntentId)
            : base(O => O.PaymentIntentId == paymentIntentId) { }
    }
}
