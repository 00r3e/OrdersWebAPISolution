using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceContracts.DTO.OrderDTO;

namespace ServiceContracts.IOrdersServices
{
    public interface IOrdersUpdaterService
    {
        Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest);
    }
}
