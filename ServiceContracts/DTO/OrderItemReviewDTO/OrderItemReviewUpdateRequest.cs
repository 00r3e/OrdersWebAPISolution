using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;

namespace ServiceContracts.DTO.OrderItemReviewDTO
{
    public class OrderItemReviewUpdateRequest
    {
        public Guid OrderItemId { get; set; }  

        public int Score { get; set; }

        public string ReviewTitle { get; set; }

        public string ReviewDescription { get; set; }

        public OrderItemReview ToOrderItemReview()
        {
            return new OrderItemReview()
            {
                OrderItemId = OrderItemId,
                Score = Score,
                ReviewTitle = ReviewTitle,
                ReviewDescription = ReviewDescription
            };
        }

    }
}
