using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;

namespace ServiceContracts.DTO.CustomersDTO
{
    public class CustomerAddRequest
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public List<Guid> CountryIds { get; set; } = new();

        /// <summary>
        /// Converts the current object of CountryAddRequest into a new object of Country type
        /// </summary>
        /// <returns></returns>
        public Customer ToCustomer()
        {
            return new Customer
            {
                FirstName = FirstName,
                LastName = LastName, 
                Email = Email,
            };
        }
    }
}
