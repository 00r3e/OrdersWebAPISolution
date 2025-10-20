using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts.DTO.OrderDTO;
using ServiceContracts.IOrdersServices;

namespace Services.OrdersServices
{
    public class OrdersAdderService : IOrdersAdderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrdersAdderService> _logger;

        public OrdersAdderService(IOrderRepository orderRepository, ILogger<OrdersAdderService> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(AddOrder), nameof(OrdersAdderService));

            Order order = orderAddRequest.ToOrder();

            order.OrderId = Guid.NewGuid();

            var currentYear = DateTime.UtcNow.Year;
            var nextSequence = await _orderRepository.GetNextOrderSequenceForYear(currentYear);

            order.OrderNumber = $"{currentYear}_{nextSequence}";

            var orderFromAdd = await _orderRepository.AddOrder(order);

            if (orderFromAdd == null)
            {
                return null;
            }

            return orderFromAdd.ToOrderResponse();
        }
    }
}
