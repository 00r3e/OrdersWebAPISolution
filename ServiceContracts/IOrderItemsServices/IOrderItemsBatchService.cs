using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceContracts.DTO.OrderItemDTO;

namespace ServiceContracts.IOrderItemsServices
{
    public interface IOrderItemsBatchService
    {
        Task<IEnumerable<OrderItemResponse>> CreateOrderItems(IEnumerable<OrderItemAddRequest> orderItemRequests);
    }
}
