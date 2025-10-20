using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using ServiceContracts.DTO.OrderItemDTO;

namespace ServiceContracts.IOrderItemsServices
{
    public interface IOrderItemsUpdaterService
    {
        Task<OrderItemResponse?> UpdateOrderItem(Guid orderId, OrderItemUpdateRequest orderItemupdateRequest);
    }
}
