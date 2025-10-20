using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceContracts.DTO.CountryDTO;
using ServiceContracts.DTO.CustomersDTO;

namespace ServiceContracts.ICustomersServices
{
    public interface ICustomersUpdaterService
    {
        Task<CustomerResponse?> UpdateCustomer(CustomerUpdateRequest customerUpdateRequest);
    }
}
