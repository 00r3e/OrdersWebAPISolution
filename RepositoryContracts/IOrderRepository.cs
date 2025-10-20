using Entities;

namespace RepositoryContracts
{
    public interface IOrderRepository
    {
        Task<ICollection<Order>> GetOrders();

        Task<Order?> GetOrder(Guid orderId);

        Task<Order?> AddOrder(Order order);

        Task<Order?> UpdateOrder(Order order);

        Task<bool> DeleteOrder(Guid orderId);

        Task<bool> Save();

        Task<int> GetNextOrderSequenceForYear(int year);
    }
}
