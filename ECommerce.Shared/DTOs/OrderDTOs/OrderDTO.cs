using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Shared.DTOs.OrderDTOs
{
    public record OrderDTO
    {
        public string BasketId { get; init; }

        public int DeliveryMethodId { get; init; }

        public AddressDTO ShipToAddress { get; init; }
    }
}
