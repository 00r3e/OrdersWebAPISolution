using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using ServiceContracts.DTO.CountryDTO;

namespace ServiceContracts.DTO.CustomersDTO
{
    public class CustomerResponse
    {
        public Guid CustomerId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public List<string> CountryNames { get; set; } = new();

    }
    public static class CustomerExtensions
    {
        public static CustomerResponse ToCustomerResponse(this Customer customer)
        {
            return new CustomerResponse()
            {
                CustomerId = customer.CustomerId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                CountryNames = customer.Countries?
                                .Select(c => c.Name)
                                .ToList() ?? new List<string>()
            };
        }
    }
}
