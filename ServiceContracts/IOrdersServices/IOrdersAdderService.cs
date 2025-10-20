using ServiceContracts.DTO.OrderDTO;

namespace ServiceContracts.IOrdersServices
{
    public interface IOrdersAdderService
    {
        Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest);
    }
}
