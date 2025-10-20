using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts.IOrderItemReviewsServices;
using Services.OrderItemsServices;

namespace Services.OrderItemsReviewsServices
{
    public class OrderItemReviewsDeleterService : IOrderItemReviewsDeleterService
    {
        private readonly IOrderItemReviewRepository _orderItemReviewRepository;
        private readonly ILogger<OrderItemReviewsDeleterService> _logger;

        public OrderItemReviewsDeleterService(IOrderItemReviewRepository orderItemReviewRepository, ILogger<OrderItemReviewsDeleterService> logger)
        {
            _orderItemReviewRepository = orderItemReviewRepository;
            _logger = logger;
        }
        public async Task<bool> DeleteOrderItemReview(Guid OrderId, Guid orderItemReviewId)
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(DeleteOrderItemReview), nameof(OrderItemReviewsDeleterService));

            bool isDeleted = await _orderItemReviewRepository.DeleteOrderItemReview(OrderId, orderItemReviewId);

            return isDeleted;
        }
    }
}
