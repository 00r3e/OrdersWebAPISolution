using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using RepositoryContracts;
using ServiceContracts.DTO.OrderItemDTO;
using ServiceContracts.IOrderItemsServices;
using UnitOfWork;

namespace Services.OrderItemsServices
{
    public class OrderItemsBatchService : IOrderItemsBatchService
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public OrderItemsBatchService(IUnitOfWorkManager unitOfWorkManager, IOrderItemRepository orderItemRepository)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _orderItemRepository = orderItemRepository;
        }

        public async Task<IEnumerable<OrderItemResponse>> CreateOrderItems(IEnumerable<OrderItemAddRequest> orderItemRequests)
        {
            List<OrderItem> orderItemsList = new();

            _unitOfWorkManager.StartUnitOfWork();

            foreach (var orderItemRequest in orderItemRequests)
            {
                var orderItem = orderItemRequest.ToOrderItem();

                var orderItemToAdd = await _orderItemRepository.AddOrderItem(orderItem);
                if (orderItemToAdd != null)
                {
                    orderItemsList.Add(orderItemToAdd);
                }
            }

            await _unitOfWorkManager.SaveChangesAsync();

            return orderItemsList.Select(oi => oi.ToOrderItemResponse()).ToList();
        }
    }
}
