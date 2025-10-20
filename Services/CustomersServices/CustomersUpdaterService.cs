using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts.DTO.CountryDTO;
using ServiceContracts.DTO.CustomersDTO;
using ServiceContracts.ICustomersServices;
using Services.CountriesServices;

namespace Services.CustomersServices
{
    public class CustomersUpdaterService : ICustomersUpdaterService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CustomersUpdaterService> _logger;

        public CustomersUpdaterService(ICustomerRepository customerRepository, ILogger<CustomersUpdaterService> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
        }
        public async Task<CustomerResponse?> UpdateCustomer(CustomerUpdateRequest customerUpdateRequest)
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(UpdateCustomer), nameof(CustomersUpdaterService));

            var customerForUpdate = customerUpdateRequest.ToCustomer();
            Customer? UpdatedCustomer = await _customerRepository.UpdateCustomer(customerForUpdate, customerUpdateRequest.CountryIds);
            if (UpdatedCustomer == null) { return null; }
            return UpdatedCustomer.ToCustomerResponse();
        }
    }
}
