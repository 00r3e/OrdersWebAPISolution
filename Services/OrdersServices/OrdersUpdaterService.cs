using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts.DTO.OrderDTO;
using ServiceContracts.IOrdersServices;

namespace Services.OrdersServices
{
    public class OrdersUpdaterService : IOrdersUpdaterService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrdersUpdaterService> _logger;

        public OrdersUpdaterService(IOrderRepository orderRepository, ILogger<OrdersUpdaterService> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }
        public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(UpdateOrder), nameof(OrdersUpdaterService));

            var order = orderUpdateRequest.ToOrder();
            var updatedOrder = await _orderRepository.UpdateOrder(order);
            if (updatedOrder == null) { return null; }
            var updatedOrderResponse = updatedOrder.ToOrderResponse();
            return updatedOrderResponse;
        }
    }
}
