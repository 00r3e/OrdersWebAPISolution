using ServiceContracts.DTO.OrderDTO;

namespace ServiceContracts.IOrdersServices
{
    public interface IOrdersBatchService
    {
        Task<IEnumerable<OrderResponse>> CreateOrders(IEnumerable<OrderAddRequest> orderRequests);
    }
}
