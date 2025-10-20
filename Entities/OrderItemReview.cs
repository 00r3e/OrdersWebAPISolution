using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class OrderItemReview
    {
        [Key]
        public Guid OrderItemId { get; set; }
        
        [ForeignKey(nameof(CustomerId))]
        [Required(ErrorMessage = "Customer Id is required")]
        public Guid CustomerId { get; set; }

        [Range(1, 5, ErrorMessage = "Score must be between 1 and 5")]
        public int Score { get; set; }
        
        [Required]
        public string ReviewTitle { get; set; }
        
        [Required]
        public string ReviewDescription { get; set; }

        public OrderItem OrderItem { get; set; }

        public Customer Customer { get; set; }

    }
}
