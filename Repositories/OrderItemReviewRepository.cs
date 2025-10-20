using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Microsoft.EntityFrameworkCore;
using Orders.WebAPI.ApplicationDbContext;
using RepositoryContracts;
using UnitOfWork;

namespace Repositories
{
    public class OrderItemReviewRepository : IOrderItemReviewRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public OrderItemReviewRepository(AppDbContext dbContext, IUnitOfWorkManager unitOfWorkManager)
        {
            _dbContext = dbContext;
            _unitOfWorkManager = unitOfWorkManager;
        }
        public async Task<OrderItemReview?> AddOrderItemReview(OrderItemReview orderItemReview)
        {
            await _dbContext.OrderItemReviews.AddAsync(orderItemReview);

            if (!_unitOfWorkManager.IsUnitOfWorkStarted)
            {
                await Save();
            }

            return orderItemReview;
        }

        public async Task<bool> DeleteOrderItemReview(Guid orderId, Guid orderItemReviewId)
        {
            OrderItemReview? orderItemReview = await _dbContext.OrderItemReviews.FirstOrDefaultAsync(temp => 
                temp.OrderItemId == orderItemReviewId && temp.OrderItem.OrderId == orderId);
            if ((orderItemReview != null))
            {
                _dbContext.OrderItemReviews.Remove(orderItemReview);

                if (!_unitOfWorkManager.IsUnitOfWorkStarted)
                {
                    await Save();
                }
                return true;
            }
            return false;
        }

        public async Task<OrderItemReview?> GetOrderItemReview(Guid orderId, Guid orderItemReviewId)
        {
            OrderItemReview? orderItemReview = await _dbContext.OrderItemReviews.FirstOrDefaultAsync(temp =>
                temp.OrderItemId == orderItemReviewId || temp.OrderItem.OrderId == orderId);

            return orderItemReview;
        }

        public async Task<ICollection<OrderItemReview>> GetOrderItemReviews()
        {
            return await _dbContext.OrderItemReviews.Include("OrderItem").Include("Customer").ToListAsync();
        }

        public async Task<OrderItemReview?> UpdateOrderItemReview(OrderItemReview orderItemReview)
        {
            OrderItemReview? matchingOrderItemReview = await _dbContext.OrderItemReviews.FirstOrDefaultAsync(temp => 
                temp.OrderItemId == orderItemReview.OrderItemId);

            if (matchingOrderItemReview == null)
            {
                return null;
            }

            matchingOrderItemReview.Score = orderItemReview.Score;
            matchingOrderItemReview.ReviewTitle = orderItemReview.ReviewTitle;
            matchingOrderItemReview.ReviewDescription = orderItemReview.ReviewDescription;


            if (!_unitOfWorkManager.IsUnitOfWorkStarted)
            {
                await Save();
            }

            return matchingOrderItemReview;
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
