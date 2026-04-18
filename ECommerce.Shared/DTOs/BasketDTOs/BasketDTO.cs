using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Shared.DTOs.BasketDTOs
{
    public class BasketDTO
    {
        public string Id { get; set; } = default!;

        public int? DeliveryMethodId { get; set; }
        public decimal ShippingPrice { get; set; }

        public string? PaymentIntentID { get; set; }

        public string? ClientSecret { get; set; }
        public ICollection<BasketItemDTO> Items { get; set; } = [];
    }
}
