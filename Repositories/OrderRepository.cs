using System;
using Microsoft.EntityFrameworkCore;
using Orders.WebAPI.ApplicationDbContext;
using Entities;
using RepositoryContracts;
using UnitOfWork;

namespace Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public OrderRepository(AppDbContext dbContext, IUnitOfWorkManager unitOfWorkManager)
        {
            _dbContext = dbContext;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<Order?> AddOrder(Order order)
        {
            await _dbContext.Orders.AddAsync(order);

            if (!_unitOfWorkManager.IsUnitOfWorkStarted)
            {
                await Save();

            }

            return order;
        }

        public async Task<bool> DeleteOrder(Guid orderId)
        {
            Order? order = await _dbContext.Orders.FirstOrDefaultAsync(temp => temp.OrderId == orderId);
            if ((order != null))
            {
                _dbContext.Orders.Remove(order);

                if (!_unitOfWorkManager.IsUnitOfWorkStarted)
                {
                    await Save();
                }
                return true;
            }
            return false;
        }

        public async Task<Order?> GetOrder(Guid orderId)
        {
            Order? order = await _dbContext.Orders.Include(o => o.Customer).FirstOrDefaultAsync(temp => temp.OrderId == orderId);

            return order;
        }

        public async Task<ICollection<Order>> GetOrders()
        {
            return await _dbContext.Orders.Include(o=>o.Items).Include(o => o.Customer).ToListAsync();
        }

        public async Task<Order?> UpdateOrder(Order order)
        {
            Order? matchingOrder = await _dbContext.Orders.FirstOrDefaultAsync(temp => temp.OrderId == order.OrderId);

            if (matchingOrder == null)
            {
                return null;
            }

            matchingOrder.OrderDate = order.OrderDate;
            matchingOrder.TotalAmount = order.TotalAmount;
            matchingOrder.CustomerId = order.CustomerId;


            if (!_unitOfWorkManager.IsUnitOfWorkStarted)
            {
                await Save();
            }

            return matchingOrder;

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

        public async Task<int> GetNextOrderSequenceForYear(int year)
        {
            var orderNumbers = await _dbContext.Orders
                .Where(o => o.OrderDate.Year == year && o.OrderNumber.StartsWith($"{year}_"))
                .Select(o => o.OrderNumber)
                .ToListAsync();

            var max = orderNumbers
                .Select(temp =>
                {
                    var parts = temp.Split('_');
                    return parts.Length == 2 && int.TryParse(parts[1], out int num) ? num : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            return max + 1;
        }
    }
}
