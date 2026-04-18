using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Domain.Contracts;
using ECommerce.Domain.Entities.BasketModule;
using ECommerce.Domain.Entities.OrderModule;
using ECommerce.Domain.Entities.ProductModule;
using ECommerce.Services.Abstraction;
using ECommerce.Services.Specifications.OrderSpecifications;
using ECommerce.Shared.CommonResponses;
using ECommerce.Shared.DTOs.OrderDTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace ECommerce.Services
{
    public class OrderService : IOrderService
    {
        private readonly IMapper _mapper;
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(
            IMapper mapper,
            IBasketRepository basketRepository,
            IUnitOfWork unitOfWork
        )
        {
            _mapper = mapper;
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<OrderToReturnDTO>> CreateOrderAsync(
            OrderDTO orderDTO,
            string email
        )
        {
            //1- Maps the provided shipping address to the order address entity.
            var orderAddress = _mapper.Map<OrderAddress>(orderDTO.ShipToAddress);

            //2-Retrieves the basket and validates its existence.
            var basket = await _basketRepository.GetBasketAsync(orderDTO.BasketId);
            if (basket is null)
                return Error.NotFound(
                    "Basket.NotFound",
                    $"The basket with Id:{orderDTO.BasketId} is Not found"
                );

            if (basket.PaymentIntentID is null)
                return Error.Validation("PaymentIntent.NotFound");

            //3-Creates a list of order items by fetching product details from the database and validating each product.
            List<OrderItem> orderItems = new List<OrderItem>();

            foreach (var item in basket.Items)
            {
                var product = await _unitOfWork.GetRepository<Product, int>().GetByIdAsync(item.Id);
                if (product is null)
                    return Error.NotFound(
                        "Product.NotFound",
                        $"The product with Id:{item.Id} is Not found"
                    );
                orderItems.Add(CreateOrderItem(item, product));
            }
            //4-Retrieves the selected delivery method and validates its existence.
            var deliveryMethod = await _unitOfWork
                .GetRepository<DeliveryMethod, int>()
                .GetByIdAsync(orderDTO.DeliveryMethodId);
            if (deliveryMethod is null)
                return Error.NotFound(
                    "DeliveryMethod.NotFound",
                    $"The Delivery Method with this Id:{orderDTO.DeliveryMethodId} is Not Found "
                );
            //5-Calculates the subtotal of the order based on the items and their quantities.
            var SubTotal = orderItems.Sum(X => X.Price * X.Quantity);

            var orderSpec = new OrderWithPaymentIntentSpecifications(basket.PaymentIntentID);
            var orderRepo = _unitOfWork.GetRepository<Order, Guid>();
            var OrderExistWithThisPaymentIntent = await orderRepo.GetByIdAsync(orderSpec);

            if (OrderExistWithThisPaymentIntent is not null)
                orderRepo.Delete(OrderExistWithThisPaymentIntent);

            //6-Creates a new Order with all relevant details.
            var order = new Order()
            {
                UserEmail = email,
                Address = orderAddress,
                DeliveryMethod = deliveryMethod,
                PaymentIntentId = basket.PaymentIntentID!,
                SubTotal = SubTotal,
                Items = orderItems,
            };

            await _unitOfWork.GetRepository<Order, Guid>().AddAsync(order); //Adding The order and orderItems Locally

            bool result = await _unitOfWork.SaveChangesAsync() > 0;
            if (!result)
                Error.Faliure("Order.Faliure", "There was a problem while creating the order");

            //7-Returns a DTO containing the full order details to the client,
            //including Id[OrderId], UserEmail,
            //items[ProductName, PictureUrl, Price, Quantity], address, delivery method[ShortName],
            //order status, OrderDate, subtotal, and total price

            return _mapper.Map<OrderToReturnDTO>(order);
        }

        public async Task<Result<IEnumerable<DeliveryMethodDTO>>> GetAllDeliveryMethodsAsync()
        {
            var deliveryMethods = await _unitOfWork
                .GetRepository<DeliveryMethod, int>()
                .GetAllAsync();

            if (!deliveryMethods.Any())
                return Error.NotFound("DeliveryMethods.NotFound", "No Delivery Methods Found");

            var data = _mapper.Map<IEnumerable<DeliveryMethod>, IEnumerable<DeliveryMethodDTO>>(
                deliveryMethods
            );

            if (data is null)
                return Error.NotFound("DeliveryMethods.NotFound", "No Delivery Methods Found");

            return Result<IEnumerable<DeliveryMethodDTO>>.Ok(data);
        }

        public async Task<Result<IEnumerable<OrderToReturnDTO>>> GetAllOrdersAsync(string email)
        {
            var OrderSpec = new OrderSpecification(email);
            var orders = await _unitOfWork.GetRepository<Order, Guid>().GetAllAsync(OrderSpec);

            if (!orders.Any())
                return Error.NotFound(
                    "Orders.NotFound",
                    $"No Orders Found for the user with email:{email}"
                );

            var Data = _mapper.Map<IEnumerable<Order>, IEnumerable<OrderToReturnDTO>>(orders);

            return Result<IEnumerable<OrderToReturnDTO>>.Ok(Data);
        }

        public async Task<Result<OrderToReturnDTO>> GetOrderByIdAsync(Guid Id, string email)
        {
            var orderSpec = new OrderSpecification(Id, email);
            var order = await _unitOfWork.GetRepository<Order, Guid>().GetByIdAsync(orderSpec);

            if (order is null)
                return Error.NotFound(
                    "Order.NotFound",
                    $"No Order Found with Id:{Id} for the user with email:{email}"
                );

            var data = _mapper.Map<Order, OrderToReturnDTO>(order);

            return Result<OrderToReturnDTO>.Ok(data);
        }

        private OrderItem CreateOrderItem(BasketItem item, Product product)
        {
            return new OrderItem()
            {
                Product = new ProductItemOrdered()
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    PictureUrl = product.PictureUrl,
                },
                Price = product.Price,
                Quantity = item.Quantity,
            };
        }
    }
}
