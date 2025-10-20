using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Entities;
using Microsoft.EntityFrameworkCore;

namespace ServiceContracts.DTO.CustomersDTO
{
    public class CustomerUpdateRequest
    {
        public Guid CustomerId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public List<Guid> CountryIds { get; set; } = new();

        /// <summary>
        /// Converts the current object of OrderAddRequest into a new object of Order type
        /// </summary>
        /// <returns></returns>
        public Customer ToCustomer()
        {
            return new Customer()
            {
                CustomerId = CustomerId,
                FirstName = FirstName,
                LastName = LastName, 
                Email = Email
            };
        }
    }
}
