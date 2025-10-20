using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.IOrderItemsServices
{
    public interface IOrderItemsDeleterService
    {
        Task<bool> DeleteOrderItem(Guid OrderId, Guid orderItemId);
    }
}
