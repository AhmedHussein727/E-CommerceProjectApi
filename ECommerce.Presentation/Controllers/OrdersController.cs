using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ECommerce.Services.Abstraction;
using ECommerce.Shared.DTOs.OrderDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Presentation.Controllers
{
    public class OrdersController : ApiBaseController
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [Authorize]
        [HttpPost]
        //POST:baseUrl/api/Orders
        public async Task<ActionResult<OrderToReturnDTO>> CreateOrder(OrderDTO orderDTO)
        {
            var result = await _orderService.CreateOrderAsync(orderDTO, GetEmailFromToken());

            return HandleResult(result);
        }

        [Authorize]
        [HttpGet]
        //GET:BaseUrl/api/Orders
        public async Task<ActionResult<IEnumerable<OrderToReturnDTO>>> GetOrders()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var result = await _orderService.GetAllOrdersAsync(GetEmailFromToken());
            return HandleResult(result);
        }

        [Authorize]
        [HttpGet("{id:guid}")]
        //GET:baseUrl/api/Orders

        public async Task<ActionResult<OrderToReturnDTO>> GetOrder(Guid id)
        {
            var result = await _orderService.GetOrderByIdAsync(id, GetEmailFromToken());
            return HandleResult(result);
        }

        [AllowAnonymous]
        [HttpGet("deliveryMethods")]
        //GET:BaseUrl/api/Orders

        public async Task<ActionResult<IEnumerable<DeliveryMethodDTO>>> GetDeliveryMethods()
        {
            var result = await _orderService.GetAllDeliveryMethodsAsync();
            return HandleResult(result);
        }
    }
}
