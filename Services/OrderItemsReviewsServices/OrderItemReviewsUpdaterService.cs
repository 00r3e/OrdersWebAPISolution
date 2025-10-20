using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts.DTO.OrderItemReviewDTO;
using ServiceContracts.IOrderItemReviewsServices;

namespace Services.OrderItemsReviewsServices
{
    public class OrderItemReviewsUpdaterService : IOrderItemReviewsUpdaterService
    {
        private readonly IOrderItemReviewRepository _orderItemReviewRepository;
        private readonly ILogger<OrderItemReviewsUpdaterService> _logger;

        public OrderItemReviewsUpdaterService(IOrderItemReviewRepository orderItemReviewRepository, ILogger<OrderItemReviewsUpdaterService> logger)
        {
            _orderItemReviewRepository = orderItemReviewRepository;
            _logger = logger;
        }
        public async Task<OrderItemReviewResponse?> UpdateOrderItemReview(Guid customerId, OrderItemReviewUpdateRequest orderItemReviewUpdateRequest)
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(UpdateOrderItemReview), nameof(OrderItemReviewUpdateRequest));

            var orderItemReviewForUpdate = orderItemReviewUpdateRequest.ToOrderItemReview();
            orderItemReviewForUpdate.CustomerId = customerId;

            OrderItemReview? UpdatedOrderItemReview = await _orderItemReviewRepository.UpdateOrderItemReview(orderItemReviewForUpdate);
            if (UpdatedOrderItemReview == null) { return null; }
            return UpdatedOrderItemReview.ToOrderItemReviewResponse();
        }
    }
}
