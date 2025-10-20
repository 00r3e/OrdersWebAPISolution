using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts.DTO.OrderItemDTO;
using ServiceContracts.IOrderItemsServices;

namespace Services.OrderItemsServices
{
    public class OrderItemsGetterService : IOrderItemsGetterService
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly ILogger<OrderItemsGetterService> _logger;

        public OrderItemsGetterService(IOrderItemRepository orderItemRepository, ILogger<OrderItemsGetterService> logger)
        {
            _orderItemRepository = orderItemRepository;
            _logger = logger;
        }
        public async Task<OrderItemResponse?> GetOrderItem(Guid orderId, Guid orderItemId)
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(GetOrderItem), nameof(OrderItemsGetterService));


            OrderItem? orderItem = await _orderItemRepository.GetOrderItem(orderId, orderItemId);

            if (orderItem == null)
            {
                return null;
            }

            return orderItem.ToOrderItemResponse();
        }

        public async Task<ICollection<OrderItemResponse>> GetOrderItemsFromOrderID(Guid orderId)
        {
            var orderItems = await _orderItemRepository.GetOrderItemsFromOrderID(orderId);

            return orderItems.Select(temp => temp.ToOrderItemResponse()).ToList();
        }
    }
}
