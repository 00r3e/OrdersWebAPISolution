using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using ServiceContracts.DTO.OrderItemDTO;

namespace ServiceContracts.DTO.OrderItemReviewDTO
{
    public class OrderItemReviewResponse
    {
        public Guid OrderItemId { get; set; }
        public Guid CustomerId { get; set; }
        public int Score { get; set; }
        public string ReviewTitle { get; set; }
        public string ReviewDescription { get; set; }

        
    }

    public static class OrderItemReviewExtensions
    {
        public static OrderItemReviewResponse ToOrderItemReviewResponse(this OrderItemReview orderItemReview)
        {
            return new OrderItemReviewResponse
            {
                OrderItemId = orderItemReview.OrderItemId,
                CustomerId = orderItemReview.CustomerId,
                Score = orderItemReview.Score,
                ReviewTitle = orderItemReview.ReviewTitle,
                ReviewDescription = orderItemReview.ReviewDescription
            };
        }
    }
}
