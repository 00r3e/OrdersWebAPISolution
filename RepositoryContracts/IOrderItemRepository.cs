using Entities;

namespace RepositoryContracts
{
    public interface IOrderItemRepository
    {
        Task<ICollection<OrderItem>> GetOrderItemsFromOrderID(Guid orderId);

        Task<OrderItem?> GetOrderItem(Guid orderId, Guid orderItemId);

        Task<OrderItem?> AddOrderItem(OrderItem orderItem);

        Task<OrderItem?> UpdateOrderItem(OrderItem orderItem);

        Task<bool> DeleteOrderItem(Guid orderId, Guid orderItemId);

        Task<bool> Save();
    }
}
