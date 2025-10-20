using Entities;
using Microsoft.EntityFrameworkCore;
using Orders.WebAPI.ApplicationDbContext;
using RepositoryContracts;
using UnitOfWork;


namespace Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public OrderItemRepository(AppDbContext dbContext, IUnitOfWorkManager unitOfWorkManager)
        {
            _dbContext = dbContext;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<OrderItem?> AddOrderItem(OrderItem orderItem)
        {
            await _dbContext.OrderItems.AddAsync(orderItem);

            if (!_unitOfWorkManager.IsUnitOfWorkStarted)
            {
                await Save();

            }

            return orderItem;
        }

        public async Task<OrderItem?> GetOrderItem(Guid orderId, Guid orderItemId)
        {
            OrderItem? orderItem = await _dbContext.OrderItems.Include(oi => oi.OrderItemReview).FirstOrDefaultAsync(temp => 
                temp.OrderItemId == orderItemId && temp.OrderId == orderId);

            return orderItem;
        }

        public async Task<ICollection<OrderItem>> GetOrderItemsFromOrderID(Guid orderId)
        {
            return await _dbContext.OrderItems.Include(oi=>oi.OrderItemReview)
                           .Where(temp => temp.OrderId == orderId)
                           .ToListAsync();
        }

        public async Task<OrderItem?> UpdateOrderItem(OrderItem orderItem)
        {
            OrderItem? matchingOrderItem = await _dbContext.OrderItems.FirstOrDefaultAsync(temp => 
                temp.OrderId == orderItem.OrderId && temp.OrderItemId == orderItem.OrderItemId);

            if (matchingOrderItem == null)
            {
                return null;
            }

            matchingOrderItem.ProductName = orderItem.ProductName;
            matchingOrderItem.Quantity = orderItem.Quantity;
            matchingOrderItem.UnitPrice = orderItem.UnitPrice;
            matchingOrderItem.TotalPrice = orderItem.TotalPrice;

            if (!_unitOfWorkManager.IsUnitOfWorkStarted)
            {
                await Save();
            }

            return matchingOrderItem;
        }

        public async Task<bool> DeleteOrderItem(Guid orderId, Guid orderItemId)
        {
            OrderItem? orderItem = await _dbContext.OrderItems.FirstOrDefaultAsync(temp => temp.OrderId == orderId && temp.OrderItemId == orderItemId);
            if (orderItem != null)
            {
                _dbContext.OrderItems.Remove(orderItem);
                if (!_unitOfWorkManager.IsUnitOfWorkStarted)
                {
                    await Save();
                }
                return true;
            }
            return false;
        }

        public async Task<bool> Save()
        {
            try
            {
                var result = await _dbContext.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
