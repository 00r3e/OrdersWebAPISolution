using Entities;
using RepositoryContracts;
using ServiceContracts.DTO.OrderDTO;
using ServiceContracts.IOrdersServices;
using UnitOfWork;

namespace Services.OrdersServices
{
    public class OrdersBatchService : IOrdersBatchService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public OrdersBatchService(IOrderRepository orderRepository, IUnitOfWorkManager unitOfWorkManager)
        {
            _orderRepository = orderRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }
        public async Task<IEnumerable<OrderResponse>> CreateOrders(IEnumerable<OrderAddRequest> orderRequests)
        {
            List<Order> ordersList = new();

            _unitOfWorkManager.StartUnitOfWork();

            int counter = 0; 
            foreach (var orderRequest in orderRequests)
            {
                var order = orderRequest.ToOrder();
                order.OrderId = Guid.NewGuid();

                var currentYear = DateTime.UtcNow.Year;
                var nextSequence = await _orderRepository.GetNextOrderSequenceForYear(currentYear);

                order.OrderNumber = $"{currentYear}_{nextSequence + counter}";
                var orderToAdd = await _orderRepository.AddOrder(order);
                if (orderToAdd != null)
                {
                    ordersList.Add(orderToAdd);
                }
                counter++;
            }

            await _unitOfWorkManager.SaveChangesAsync();

            return ordersList.Select(o => o.ToOrderResponse()).ToList();
        }
    }
}
