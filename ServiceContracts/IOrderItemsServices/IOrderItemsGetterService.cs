using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using ServiceContracts.DTO.OrderItemDTO;

namespace ServiceContracts.IOrderItemsServices
{
    public interface IOrderItemsGetterService
    {
        Task<ICollection<OrderItemResponse>> GetOrderItemsFromOrderID(Guid orderId);

        Task<OrderItemResponse?> GetOrderItem(Guid orderId, Guid orderItemId);
    }
}
