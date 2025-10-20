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
    public class OrdersGetterService : IOrdersGetterService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrdersGetterService> _logger;

        public OrdersGetterService(IOrderRepository ordersGetterService, ILogger<OrdersGetterService> logger)
        {
            _orderRepository = ordersGetterService;
            _logger = logger;
        }
        public async Task<OrderResponse?> GetOrder(Guid orderId)
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(GetOrder), nameof(OrdersGetterService));


            var order = await _orderRepository.GetOrder(orderId);

            if (order == null) { return null; }

            return order.ToOrderResponse();
        }

        public async Task<ICollection<OrderResponse>> GetOrders()
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(GetOrders), nameof(OrdersGetterService));

            var orders = await _orderRepository.GetOrders();

            var orderResponses = orders.Select(temp => temp.ToOrderResponse()).ToList();

            return orderResponses;
        }

    }
}
