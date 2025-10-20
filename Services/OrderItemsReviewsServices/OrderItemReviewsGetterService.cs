using Entities;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts.DTO.OrderItemReviewDTO;
using ServiceContracts.IOrderItemReviewsServices;
using Services.OrderItemsServices;

namespace Services.OrderItemsReviewsServices
{
    public class OrderItemReviewsGetterService : IOrderItemReviewsGetterService
    {
        private readonly IOrderItemReviewRepository _orderItemReviewRepository;
        private readonly ILogger<OrderItemsGetterService> _logger;

        public OrderItemReviewsGetterService(IOrderItemReviewRepository orderItemReviewRepository, ILogger<OrderItemsGetterService> logger)
        {
            _orderItemReviewRepository = orderItemReviewRepository;
            _logger = logger;
        }
        public async Task<OrderItemReviewResponse?> GetOrderItemReview(Guid orderId, Guid orderItemId)
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(GetOrderItemReview), nameof(OrderItemReviewsGetterService));


            OrderItemReview? orderItemReview = await _orderItemReviewRepository.GetOrderItemReview(orderId, orderItemId);

            if (orderItemReview == null)
            {
                return null;
            }

            return orderItemReview.ToOrderItemReviewResponse();
        }

    }
}
