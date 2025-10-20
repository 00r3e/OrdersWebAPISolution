using System.ComponentModel.DataAnnotations;
using Entities;

namespace ServiceContracts.DTO.OrderDTO
{
    public class OrderResponse
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; }
        public Guid CustomerId{ get; set; }
        public DateTime OrderDate { get; set; }
        public int TotalAmount { get; set; }
    }

    public static class OrderExtensions
    {
        public static OrderResponse ToOrderResponse(this Order order)
        {
            return new OrderResponse()
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount

            };
        }
    }
}
