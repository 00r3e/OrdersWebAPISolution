using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities
{
    public class OrderItem
    {
        [Key]
        public Guid OrderItemId { get; set; }

        [ForeignKey(nameof(OrderId))]
        [Required(ErrorMessage = "Order Id is required")]
        public Guid OrderId { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [MaxLength(50, ErrorMessage = "Product name can't be more than 50 characters")]
        public string ProductName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive number")]
        public int Quantity { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Unit price must be a positive number")]
        public int UnitPrice { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Total price must be a positive number")]
        public int TotalPrice { get; set; }

        public Order Order { get; set; }

        public OrderItemReview? OrderItemReview { get; set; }
    }
}
