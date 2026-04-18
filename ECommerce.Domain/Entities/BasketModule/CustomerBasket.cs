using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domain.Entities.BasketModule
{
    public class CustomerBasket
    {
        public string Id { get; set; } = default!; //Created from Front-end [GUID]

        public int? DeliveryMethodId { get; set; }
        public decimal ShippingPrice { get; set; }

        public string? PaymentIntentID { get; set; }

        public string? ClientSecret { get; set; }
        public ICollection<BasketItem> Items { get; set; } = [];
    }
}
