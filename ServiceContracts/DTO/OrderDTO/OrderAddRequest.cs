using Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace ServiceContracts.DTO.OrderDTO
{
    public class OrderAddRequest
    {

        public int TotalAmount { get; set; }

        public Guid CustomerId { get; set; }

        /// <summary>
        /// Converts the current object of OrderAddRequest into a new object of Order type
        /// </summary>
        /// <returns></returns>
        public Order ToOrder()
        {
            return new Order
            {
                CustomerId = CustomerId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = TotalAmount
            };
        }
    }
}
