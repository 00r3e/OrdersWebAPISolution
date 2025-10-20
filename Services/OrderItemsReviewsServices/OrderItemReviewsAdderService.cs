
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts.DTO.OrderItemReviewDTO;
using ServiceContracts.IOrderItemReviewsServices;

namespace Services.OrderItemsReviewsServices
{
    public class OrderItemReviewsAdderService : IOrderItemReviewsAdderService
    {
        private readonly IOrderItemReviewRepository _orderItemReviewRepository;
        private readonly ILogger<OrderItemReviewsAdderService> _logger;

        public OrderItemReviewsAdderService(IOrderItemReviewRepository orderItemReviewRepository, ILogger<OrderItemReviewsAdderService> logger)
        {
            _orderItemReviewRepository = orderItemReviewRepository;
            _logger = logger;
        }
        public async Task<OrderItemReviewResponse?> AddOrderItemReview(Guid customerId, OrderItemReviewAddRequest orderItemReviewAddRequest)
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(AddOrderItemReview), nameof(OrderItemReviewsAdderService));

            var orderItemReview = orderItemReviewAddRequest.ToOrderItemReview();

            orderItemReview.CustomerId = customerId;

            var orderFromAdd = await _orderItemReviewRepository.AddOrderItemReview(orderItemReview);

            if (orderFromAdd == null) { return null; }

            return orderFromAdd.ToOrderItemReviewResponse();
        }
    }
}
