using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceContracts.DTO.CustomersDTO;
using ServiceContracts.DTO.OrderItemDTO;

namespace ServiceContracts.ICustomersServices
{
    public interface ICustomersAdderService
    {
        Task<CustomerResponse?> AddCustomer(CustomerAddRequest customerAddRequest);
    }
}
