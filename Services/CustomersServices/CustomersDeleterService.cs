using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts.ICustomersServices;
using Services.CountriesServices;

namespace Services.CustomersServices
{
    public class CustomersDeleterService : ICustomersDeleterService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CustomersDeleterService> _logger;

        public CustomersDeleterService(ICustomerRepository customerRepository, ILogger<CustomersDeleterService> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
        }

        public async Task<bool> DeleteCustomer(Guid customerId)
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(DeleteCustomer), nameof(CustomersDeleterService));

            bool isDeleted = await _customerRepository.DeleteCustomer(customerId);

            return isDeleted;
        }
    }
}
