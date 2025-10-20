using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.IOrdersServices
{
    public interface IOrdersDeleterService
    {
        Task<bool> DeleteOrder(Guid orderId);
    }
}
