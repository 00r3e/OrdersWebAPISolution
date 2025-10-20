using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Country
    {
        [Key] 
        public Guid CountryId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        public List<Customer> Customers { get; set; } = new();
    }
}
