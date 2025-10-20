using System.ComponentModel.DataAnnotations;

namespace Entities
{
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; }
        [Required(ErrorMessage = "Order number is required")]
        public string OrderNumber { get; set; }

        [Required(ErrorMessage = "Customer Id is required")]
        public Guid CustomerId{ get; set; }
        
        [Required(ErrorMessage = "Date of order is required")]
        public DateTime OrderDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage ="Total amount must be a positive number")]
        public int TotalAmount { get; set; }

        public Customer Customer { get; set; } 

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    }
}
