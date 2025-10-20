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
    public class OrderItemsUpdaterService : IOrderItemsUpdaterService
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderItemsUpdaterService> _logger;

        public OrderItemsUpdaterService(IOrderItemRepository orderItemRepository, IOrderRepository orderRepository,  ILogger<OrderItemsUpdaterService> logger)
            
        {
            _orderItemRepository = orderItemRepository;
            _orderRepository = orderRepository;
            _logger = logger;
        }
        public async Task<OrderItemResponse?> UpdateOrderItem(Guid orderId, OrderItemUpdateRequest orderItemUpdateRequest)
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(UpdateOrderItem), nameof(OrderItemsUpdaterService));

            var orderItemForUpdate = orderItemUpdateRequest.ToOrderItem();
            orderItemForUpdate.OrderId = orderId;
            OrderItem? UpdatedOrderItem = await _orderItemRepository.UpdateOrderItem(orderItemForUpdate);
            if (UpdatedOrderItem == null) { return null; }
            return UpdatedOrderItem.ToOrderItemResponse();
        }
    }
}
