using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Shared.DTOs.OrderDTOs
{
    public record DeliveryMethodDTO
    {
        public int Id { get; init; }
        public string ShortName { get; init; } = default!;
        public string Description { get; init; } = default!;

        public string DeliveryTime { get; init; } = default!;

        public decimal Price { get; init; }
    }
}
