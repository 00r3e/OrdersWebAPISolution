using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;

namespace RepositoryContracts
{
    public interface IOrderItemReviewRepository
    {
        Task<ICollection<OrderItemReview>> GetOrderItemReviews();

        Task<OrderItemReview?> GetOrderItemReview(Guid orderId, Guid orderItemReviewId);

        Task<OrderItemReview?> AddOrderItemReview(OrderItemReview orderItemReview);

        Task<OrderItemReview?> UpdateOrderItemReview(OrderItemReview orderItemReview);

        Task<bool> DeleteOrderItemReview(Guid orderId, Guid orderItemReviewId);

        Task<bool> Save();
    }
}
