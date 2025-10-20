using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts.DTO.OrderItemDTO;
using ServiceContracts.IOrderItemsServices;

namespace Services.OrderItemsServices
{
    public class OrderItemsAdderService : IOrderItemsAdderService
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly ILogger<OrderItemsAdderService> _logger;

        public OrderItemsAdderService(IOrderItemRepository orderItemRepository, ILogger<OrderItemsAdderService> logger)
        {
            _orderItemRepository = orderItemRepository;
            _logger = logger;
        }

        public async Task<OrderItemResponse?> AddOrderItem(OrderItemAddRequest orderItemAddRequest)
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(AddOrderItem), nameof(OrderItemsAdderService));


            var orderItem = orderItemAddRequest.ToOrderItem();

            var orderFromAdd = await _orderItemRepository.AddOrderItem(orderItem);

            if (orderFromAdd == null) { return null; }

            return orderFromAdd.ToOrderItemResponse();
        }
    }
}
