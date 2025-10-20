using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts.DTO.CustomersDTO;
using ServiceContracts.ICustomersServices;
using Services.OrderItemsServices;

namespace Services.CustomersServices
{
    public class CustomersAdderService : ICustomersAdderService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CustomersAdderService> _logger;

        public CustomersAdderService(ICustomerRepository customerRepository, ILogger<CustomersAdderService> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
        }

        public async Task<CustomerResponse?> AddCustomer(CustomerAddRequest customerAddRequest)
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(AddCustomer), nameof(CustomersAdderService));


            var customer = customerAddRequest.ToCustomer();

            customer.CustomerId = Guid.NewGuid();

            var customerFromAdd = await _customerRepository.AddCustomer(customer, customerAddRequest.CountryIds);

            if (customerFromAdd == null) { return null; }

            return customerFromAdd.ToCustomerResponse();
        }
    }
}
