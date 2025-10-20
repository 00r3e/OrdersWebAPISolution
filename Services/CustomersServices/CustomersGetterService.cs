using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts.DTO.CustomersDTO;
using ServiceContracts.ICustomersServices;
using Services.CountriesServices;

namespace Services.CustomersServices
{
    public class CustomersGetterService : ICustomersGetterService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CustomersGetterService> _logger;

        public CustomersGetterService(ICustomerRepository customerRepository, ILogger<CustomersGetterService> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
        }
        public async Task<ICollection<CustomerResponse>> GetAllCustomers()
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(GetAllCustomers), nameof(CustomersGetterService));
            var customers = await _customerRepository.GetCustomers();

            return customers.Select(temp => temp.ToCustomerResponse()).ToList();
        }

        public async Task<CustomerResponse?> GetCustomer(Guid customerId)
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(GetCustomer), nameof(CustomersGetterService));

            Customer? customer = await _customerRepository.GetCustomer(customerId);

            if (customer == null)
            {
                return null;
            }

            return customer.ToCustomerResponse();
        }
    }
}
