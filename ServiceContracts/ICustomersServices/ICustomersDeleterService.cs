using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.ICustomersServices
{
    public interface ICustomersDeleterService
    {
        Task<bool> DeleteCustomer(Guid customerId);
    }
}
