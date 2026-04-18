using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.Shared.CommonResponses;
using ECommerce.Shared.DTOs.OrderDTOs;

namespace ECommerce.Services.Abstraction
{
    public interface IOrderService
    {
        //Create Order (OrderDTO,string Email) => OrderToReturnDTO

        Task<Result<OrderToReturnDTO>> CreateOrderAsync(OrderDTO orderDTO, string email);

        //Get Delivery Methods

        Task<Result<IEnumerable<DeliveryMethodDTO>>> GetAllDeliveryMethodsAsync();

        //Get All OrdersForUser (string Email) => List<OrderToReturnDTO>
        Task<Result<IEnumerable<OrderToReturnDTO>>> GetAllOrdersAsync(string email);

        //Get OrderByIdForUser (string Email,Guid OrderId) => OrderToReturnDTO

        Task<Result<OrderToReturnDTO>> GetOrderByIdAsync(Guid Id, string email);
    }
}
