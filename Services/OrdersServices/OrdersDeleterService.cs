using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts.IOrdersServices;

namespace Services.OrdersServices
{
    public class OrdersDeleterService : IOrdersDeleterService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrdersDeleterService> _logger;
        public OrdersDeleterService(IOrderRepository orderRepository, ILogger<OrdersDeleterService> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }
        public async Task<bool> DeleteOrder(Guid orderId)
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(DeleteOrder), nameof(OrdersDeleterService));

            bool isDeleter = await _orderRepository.DeleteOrder(orderId);
            return isDeleter;
        }
    }
}
