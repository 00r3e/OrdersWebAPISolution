using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts.IOrderItemsServices;

namespace Services.OrderItemsServices
{
    public class OrderItemsDeleterService : IOrderItemsDeleterService
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly ILogger<OrderItemsDeleterService> _logger;

        public OrderItemsDeleterService(IOrderItemRepository orderItemRepository, ILogger<OrderItemsDeleterService> logger)
        {
            _orderItemRepository = orderItemRepository;
            _logger = logger;
        }
        public async Task<bool> DeleteOrderItem(Guid OrderId, Guid orderItemId)
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(DeleteOrderItem), nameof(OrderItemsDeleterService));


            bool isDeleted = await _orderItemRepository.DeleteOrderItem(OrderId, orderItemId);

            return isDeleted;
        }
    }
}
