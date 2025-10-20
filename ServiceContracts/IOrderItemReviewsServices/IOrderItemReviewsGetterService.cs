using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using ServiceContracts.DTO.OrderItemDTO;
using ServiceContracts.DTO.OrderItemReviewDTO;

namespace ServiceContracts.IOrderItemReviewsServices
{
    public interface IOrderItemReviewsGetterService
    {
        Task<OrderItemReviewResponse?> GetOrderItemReview(Guid orderId, Guid orderItemId);
    }
}
