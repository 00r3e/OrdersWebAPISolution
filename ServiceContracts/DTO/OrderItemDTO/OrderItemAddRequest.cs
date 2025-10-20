using Entities;

namespace ServiceContracts.DTO.OrderItemDTO
{
    public class OrderItemAddRequest
    {
        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public Guid OrderId { get; set; }

        public int UnitPrice { get; set; }

        public OrderItem ToOrderItem()
        {
            return new OrderItem()
            {
                ProductName = ProductName,
                OrderId = OrderId,
                Quantity = Quantity,
                UnitPrice = UnitPrice,
                TotalPrice = Quantity * UnitPrice
            };
        }
    }
}
