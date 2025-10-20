using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceContracts.DTO.OrderItemDTO;
using ServiceContracts.DTO.OrderItemReviewDTO;

namespace ServiceContracts.IOrderItemReviewsServices
{
    public interface IOrderItemReviewsUpdaterService
    {
        Task<OrderItemReviewResponse?> UpdateOrderItemReview(Guid customerId, OrderItemReviewUpdateRequest orderItemReviewUpdateRequest);
    }
}
