using Entities;

namespace ServiceContracts.DTO.OrderDTO
{
    public class OrderUpdateRequest
    {
        public Guid OrderId { get; set; }

        public Guid CustomerId { get; set; }

        public int TotalAmount { get; set; }

        /// <summary>
        /// Converts the current object of OrderAddRequest into a new object of Order type
        /// </summary>
        /// <returns></returns>
        public Order ToOrder()
        {
            return new Order
            {
                OrderId = OrderId,
                OrderDate = DateTime.UtcNow,
                CustomerId = CustomerId,
                TotalAmount = TotalAmount
            };
        }
    }
}
