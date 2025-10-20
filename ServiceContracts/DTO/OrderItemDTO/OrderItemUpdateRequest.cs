using Entities;

namespace ServiceContracts.DTO.OrderItemDTO
{
    public class OrderItemUpdateRequest
    {
        public Guid OrderItemId { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public int UnitPrice { get; set; }

        public OrderItem ToOrderItem()
        {
            return new OrderItem()
            {
                OrderItemId = OrderItemId,
                ProductName = ProductName,
                Quantity = Quantity,
                UnitPrice = UnitPrice,
                TotalPrice = Quantity * UnitPrice
            };
        }
    }
}
