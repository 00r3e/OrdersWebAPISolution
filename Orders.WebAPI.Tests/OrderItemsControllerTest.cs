using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Orders.WebAPI.Controllers;
using ServiceContracts.DTO.OrderDTO;
using ServiceContracts.DTO.OrderItemDTO;
using ServiceContracts.IOrderItemsServices;
using ServiceContracts.IOrdersServices;
using Services;

namespace Orders.WebAPI.Tests
{
    public class OrderItemsControllerTest
    {
        private readonly Mock<IOrderItemsAdderService> _orderItemsAdderServiceMock;
        private readonly Mock<IOrderItemsDeleterService> _orderItemsDeleterServiceMock;
        private readonly Mock<IOrderItemsGetterService> _orderItemsGetterServiceMock;
        private readonly Mock<IOrderItemsUpdaterService> _orderItemsUpdaterServiceMock;
        private readonly Mock<IOrdersGetterService> _ordersGetterServiceMock;
        private readonly Mock<IOrderItemsBatchService> _orderItemsBatchServiceMock;
        private readonly Mock<ILogger<OrderItemsController>> _loggerMock;

        private readonly Fixture _fixture;

        public OrderItemsControllerTest()
        {
            _fixture = new Fixture();

            // Handle circular references
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _orderItemsAdderServiceMock = new Mock<IOrderItemsAdderService>();
            _orderItemsDeleterServiceMock = new Mock<IOrderItemsDeleterService>();
            _orderItemsGetterServiceMock = new Mock<IOrderItemsGetterService>();
            _orderItemsUpdaterServiceMock = new Mock<IOrderItemsUpdaterService>();
            _ordersGetterServiceMock = new Mock<IOrdersGetterService>();
            _orderItemsBatchServiceMock = new Mock<IOrderItemsBatchService>();
            _loggerMock = new Mock<ILogger<OrderItemsController>>();
        }

        private OrderItemsController CreateController()
        {
            return new OrderItemsController(
                _loggerMock.Object,
                _orderItemsDeleterServiceMock.Object,
                _orderItemsAdderServiceMock.Object,
                _orderItemsGetterServiceMock.Object,
                _orderItemsUpdaterServiceMock.Object,
                _ordersGetterServiceMock.Object,
                _orderItemsBatchServiceMock.Object
            );
        }

        #region GET /api/orders/{orderId}/items

        [Fact]
        public async Task GetOrderItemsFromOrderId_ShouldReturnOk_WithOrderItemsList()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderItems = _fixture.CreateMany<OrderItemResponse>(5).ToList();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync(_fixture.Build<OrderResponse>().With(x => x.OrderId, orderId).Create());

            _orderItemsGetterServiceMock
                .Setup(s => s.GetOrderItemsFromOrderID(orderId))
                .ReturnsAsync(orderItems);

            var controller = CreateController();

            // Act
            var result = await controller.GetOrderItemsFromOrderId(orderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<ICollection<OrderItemResponse>>(okResult.Value);
            returnValue.Should().BeEquivalentTo(orderItems);
        }

        #endregion

        #region GET /api/orders/{orderId}/items/{orderItemId}

        [Fact]
        public async Task GetOrderItem_ShouldReturnOk_WithOrderItem()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderItemId = Guid.NewGuid();

            var orderResponse = _fixture.Build<OrderResponse>()
                                        .With(x => x.OrderId, orderId)
                                        .Create();

            var orderItemResponse = _fixture.Build<OrderItemResponse>()
                                            .With(x => x.OrderId, orderId)
                                            .With(x => x.OrderItemId, orderItemId)
                                            .Create();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync(orderResponse);

            _orderItemsGetterServiceMock
                .Setup(s => s.GetOrderItem(orderId, orderItemId))
                .ReturnsAsync(orderItemResponse);

            var controller = CreateController();

            // Act
            var result = await controller.GetOrderItem(orderId, orderItemId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<OrderItemResponse>(okResult.Value);
            returnValue.Should().BeEquivalentTo(orderItemResponse);
        }

        #endregion

        #region PUT /api/orders/{orderId}/items/{orderItemId}

        [Fact]
        public async Task PutOrderItem_ShouldReturnCreatedAtActionResult_WithUpdatedOrderItem()
        {
            // Arrange
            var updateRequest = _fixture.Create<OrderItemUpdateRequest>();
            var orderItemResponse = _fixture.Create<OrderItemResponse>();
            var orderResponse = _fixture.Create<OrderResponse>();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(It.IsAny<Guid>()))
                .ReturnsAsync(orderResponse);

            _orderItemsGetterServiceMock
                .Setup(s => s.GetOrderItem(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(orderItemResponse);

            _orderItemsUpdaterServiceMock
                .Setup(s => s.UpdateOrderItem(It.IsAny<Guid>(), It.IsAny<OrderItemUpdateRequest>()))
                .ReturnsAsync(orderItemResponse);

            var controller = CreateController();

            // Act
            var result = await controller.PutOrderItem(orderResponse.OrderId, updateRequest.OrderItemId, updateRequest);

            // Assert
            var createdAtAction = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(OrderItemsController.GetOrderItem), createdAtAction.ActionName);
            var actualResponse = Assert.IsType<OrderItemResponse>(createdAtAction.Value);
            actualResponse.Should().BeEquivalentTo(orderItemResponse);
        }

        #endregion

        #region POST /api/orders/{orderId}/items

        [Fact]
        public async Task PostOrderItem_ShouldReturnCreatedAtActionResult_WithCreatedOrderItem()
        {
            // Arrange
            var addRequest = _fixture.Create<OrderItemAddRequest>();
            var orderResponse = _fixture.Build<OrderResponse>()
                                        .With(o => o.OrderId, addRequest.OrderId)
                                        .Create();
            var orderItemResponse = _fixture.Build<OrderItemResponse>()
                                            .With(o => o.OrderId, addRequest.OrderId)
                                            .Create();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(addRequest.OrderId))
                .ReturnsAsync(orderResponse);

            _orderItemsAdderServiceMock
                .Setup(s => s.AddOrderItem(addRequest))
                .ReturnsAsync(orderItemResponse);

            var controller = CreateController();

            // Act
            var result = await controller.PostOrderItem(orderResponse.OrderId, addRequest);

            // Assert
            var createdAtAction = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(OrderItemsController.GetOrderItem), createdAtAction.ActionName);
            var actualResponse = Assert.IsType<OrderItemResponse>(createdAtAction.Value);
            actualResponse.Should().BeEquivalentTo(orderItemResponse);
        }

        #endregion

        #region DELETE /api/orders/{orderId}/items/{orderItemId}

        [Fact]
        public async Task DeleteOrderItem_ShouldReturnNoContentResult()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderItemId = Guid.NewGuid();

            var orderItemResponse = _fixture.Build<OrderItemResponse>()
                                            .With(x => x.OrderId, orderId)
                                            .With(x => x.OrderItemId, orderItemId)
                                            .Create();

            _orderItemsGetterServiceMock
                .Setup(s => s.GetOrderItem(orderId, orderItemId))
                .ReturnsAsync(orderItemResponse);

            _orderItemsDeleterServiceMock
                .Setup(s => s.DeleteOrderItem(orderId, orderItemId))
                .ReturnsAsync(true);

            var controller = CreateController();

            // Act
            var result = await controller.DeleteOrderItem(orderId, orderItemId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        #endregion

        #region POST /api/orders/{orderId}/items/batch

        [Fact]
        public async Task BatchOrderItems_ShouldReturnCreatedAtActionResult_WithCreatedOrderItemsList()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderResponse = _fixture.Build<OrderResponse>().With(x => x.OrderId, orderId).Create();
            var orderItemResponses = _fixture.Build<OrderItemResponse>().With(x => x.OrderId, orderId).CreateMany(2).ToList();

            var addRequests = new List<OrderItemAddRequest>
            {
                new OrderItemAddRequest { OrderId = orderId },
                new OrderItemAddRequest { OrderId = orderId }
            };

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync(orderResponse);

            _orderItemsBatchServiceMock
                .Setup(s => s.CreateOrderItems(addRequests))
                .ReturnsAsync(orderItemResponses);

            var controller = CreateController();

            // Act
            var result = await controller.BatchOrderItems(orderId, addRequests);

            // Assert
            var createdAtAction = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(OrderItemsController.GetOrderItemsFromOrderId), createdAtAction.ActionName);
            var actualList = Assert.IsType<List<OrderItemResponse>>(createdAtAction.Value);
            actualList.Should().BeEquivalentTo(orderItemResponses);
        }

        #endregion

        #region GET /api/orders/{orderId}/items - Negative

        [Fact]
        public async Task GetOrderItemsFromOrderId_ShouldReturnBadRequest_WhenOrderIdIsEmpty()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.GetOrderItemsFromOrderId(Guid.Empty);

            // Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task GetOrderItemsFromOrderId_ShouldReturnBadRequest_WhenOrderNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync((OrderResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.GetOrderItemsFromOrderId(orderId);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            badRequest.Value.Should().Be("OrderId does not match with any Order Id from Order Database.");
        }

        #endregion

        #region GET /api/orders/{orderId}/items/{orderItemId} - Negative

        [Fact]
        public async Task GetOrderItem_ShouldReturnBadRequest_WhenIdsAreEmpty()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.GetOrderItem(Guid.Empty, Guid.Empty);

            // Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task GetOrderItem_ShouldReturnBadRequest_WhenOrderNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderItemId = Guid.NewGuid();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync((OrderResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.GetOrderItem(orderId, orderItemId);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            badRequest.Value.Should().Be("OrderId does not match with any Order Id from Order Database.");
        }

        [Fact]
        public async Task GetOrderItem_ShouldReturnNotFound_WhenOrderItemNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderItemId = Guid.NewGuid();
            var orderResponse = _fixture.Build<OrderResponse>().With(x => x.OrderId, orderId).Create();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync(orderResponse);

            _orderItemsGetterServiceMock
                .Setup(s => s.GetOrderItem(orderId, orderItemId))
                .ReturnsAsync((OrderItemResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.GetOrderItem(orderId, orderItemId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        #endregion

        #region PUT /api/orders/{orderId}/items/{orderItemId} - Negative

        [Fact]
        public async Task PutOrderItem_ShouldReturnBadRequest_WhenBodyIsNull()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.PutOrderItem(Guid.NewGuid(), Guid.NewGuid(), null);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be("Order data is missing.");
        }

        [Fact]
        public async Task PutOrderItem_ShouldReturnBadRequest_WhenIdsDoNotMatch()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderItemId = Guid.NewGuid();
            var request = _fixture.Create<OrderItemUpdateRequest>();

            var controller = CreateController();

            // Act
            var result = await controller.PutOrderItem(orderId, orderItemId, request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be("orderItemId does not match");
        }

        [Fact]
        public async Task PutOrderItem_ShouldReturnNotFound_WhenOrderNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var request = _fixture.Build<OrderItemUpdateRequest>().With(r => r.OrderItemId, Guid.NewGuid()).Create();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync((OrderResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.PutOrderItem(orderId, request.OrderItemId, request);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            notFound.Value.Should().Be("Order Id not found");
        }

        [Fact]
        public async Task PutOrderItem_ShouldReturnNotFound_WhenOrderItemNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var request = _fixture.Build<OrderItemUpdateRequest>().With(r => r.OrderItemId, Guid.NewGuid()).Create();
            var orderResponse = _fixture.Build<OrderResponse>().With(o => o.OrderId, orderId).Create();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync(orderResponse);

            _orderItemsGetterServiceMock
                .Setup(s => s.GetOrderItem(orderId, request.OrderItemId))
                .ReturnsAsync((OrderItemResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.PutOrderItem(orderId, request.OrderItemId, request);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            notFound.Value.Should().Be("Order Item Id not found");
        }

        [Fact]
        public async Task PutOrderItem_ShouldReturnProblem_WhenUpdateFails()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var request = _fixture.Build<OrderItemUpdateRequest>().With(r => r.OrderItemId, Guid.NewGuid()).Create();
            var orderResponse = _fixture.Build<OrderResponse>().With(o => o.OrderId, orderId).Create();
            var orderItemResponse = _fixture.Create<OrderItemResponse>();

            _ordersGetterServiceMock.Setup(s => s.GetOrder(orderId)).ReturnsAsync(orderResponse);
            _orderItemsGetterServiceMock.Setup(s => s.GetOrderItem(orderId, request.OrderItemId)).ReturnsAsync(orderItemResponse);
            _orderItemsUpdaterServiceMock.Setup(s => s.UpdateOrderItem(orderId, request)).ReturnsAsync((OrderItemResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.PutOrderItem(orderId, request.OrderItemId, request);

            // Assert
            var problem = Assert.IsType<ObjectResult>(result);
            var details = Assert.IsType<ProblemDetails>(problem.Value);
            details.Detail.Should().Be("An error occurred while saving the order.");
        }

        #endregion

        #region POST /api/orders/{orderId}/items - Negative

        [Fact]
        public async Task PostOrderItem_ShouldReturnBadRequest_WhenIdsDoNotMatch()
        {
            // Arrange
            var request = _fixture.Create<OrderItemAddRequest>();
            var controller = CreateController();

            // Act
            var result = await controller.PostOrderItem(Guid.NewGuid(), request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be("Order Id from Route does not match with any Order Id from Body.");
        }

        [Fact]
        public async Task PostOrderItem_ShouldReturnBadRequest_WhenBodyIsNull()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.PostOrderItem(Guid.NewGuid(), (OrderItemAddRequest?)null);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be("Order data is missing.");
        }

        [Fact]
        public async Task PostOrderItem_ShouldReturnBadRequest_WhenOrderNotFound()
        {
            // Arrange
            var request = _fixture.Create<OrderItemAddRequest>();
            _ordersGetterServiceMock.Setup(s => s.GetOrder(request.OrderId)).ReturnsAsync((OrderResponse?)null);
            var controller = CreateController();

            // Act
            var result = await controller.PostOrderItem(request.OrderId, request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be("OrderId does not match with any Order Id from Order Database.");
        }

        [Fact]
        public async Task PostOrderItem_ShouldReturnProblem_WhenAddFails()
        {
            // Arrange
            var request = _fixture.Create<OrderItemAddRequest>();
            var orderResponse = _fixture.Build<OrderResponse>().With(o => o.OrderId, request.OrderId).Create();

            _ordersGetterServiceMock.Setup(s => s.GetOrder(request.OrderId)).ReturnsAsync(orderResponse);
            _orderItemsAdderServiceMock.Setup(s => s.AddOrderItem(request)).ReturnsAsync((OrderItemResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.PostOrderItem(request.OrderId, request);

            // Assert
            var problem = Assert.IsType<ObjectResult>(result);
            var details = Assert.IsType<ProblemDetails>(problem.Value);
            details.Detail.Should().Be("An error occurred while saving the order.");
        }

        #endregion

        #region DELETE /api/orders/{orderId}/items/{orderItemId} - Negative

        [Fact]
        public async Task DeleteOrderItem_ShouldReturnNotFound_WhenOrderItemNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderItemId = Guid.NewGuid();

            _orderItemsGetterServiceMock
                .Setup(s => s.GetOrderItem(orderId, orderItemId))
                .ReturnsAsync((OrderItemResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.DeleteOrderItem(orderId, orderItemId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteOrderItem_ShouldReturnProblem_WhenDeletionFails()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderItemId = Guid.NewGuid();
            var orderItemResponse = _fixture.Build<OrderItemResponse>()
                                            .With(x => x.OrderId, orderId)
                                            .With(x => x.OrderItemId, orderItemId)
                                            .Create();

            _orderItemsGetterServiceMock.Setup(s => s.GetOrderItem(orderId, orderItemId)).ReturnsAsync(orderItemResponse);
            _orderItemsDeleterServiceMock.Setup(s => s.DeleteOrderItem(orderId, orderItemId)).ReturnsAsync(false);

            var controller = CreateController();

            // Act
            var result = await controller.DeleteOrderItem(orderId, orderItemId);

            // Assert
            var problem = Assert.IsType<ObjectResult>(result);
            var details = Assert.IsType<ProblemDetails>(problem.Value);
            details.Detail.Should().Be("An error occurred while deleting the order.");
        }

        #endregion

        #region POST /api/orders/{orderId}/items/batch - Negative

        [Fact]
        public async Task BatchOrderItems_ShouldReturnBadRequest_WhenAnyOrderIdDoesNotMatch()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var requests = new List<OrderItemAddRequest>
    {
        new OrderItemAddRequest { OrderId = orderId },
        new OrderItemAddRequest { OrderId = Guid.NewGuid() } // mismatch
    };

            var controller = CreateController();

            // Act
            var result = await controller.BatchOrderItems(orderId, requests);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be("Order Id from Route does not match with all Order Ids from Body.");
        }

        [Fact]
        public async Task BatchOrderItems_ShouldReturnBadRequest_WhenBodyIsEmpty()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var controller = CreateController();

            // Act
            var result = await controller.BatchOrderItems(orderId, new List<OrderItemAddRequest>());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be("Order Items data is missing.");
        }

        [Fact]
        public async Task BatchOrderItems_ShouldReturnBadRequest_WhenOrderNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var requests = new List<OrderItemAddRequest> { new OrderItemAddRequest { OrderId = orderId } };

            _ordersGetterServiceMock.Setup(s => s.GetOrder(orderId)).ReturnsAsync((OrderResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.BatchOrderItems(orderId, requests);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be("Order Id does not match with any Order Id from Order Database.");
        }

        [Fact]
        public async Task BatchOrderItems_ShouldReturnProblem_WhenBatchCreationFails()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderResponse = _fixture.Build<OrderResponse>().With(x => x.OrderId, orderId).Create();
            var requests = new List<OrderItemAddRequest> { new OrderItemAddRequest { OrderId = orderId } };

            _ordersGetterServiceMock.Setup(s => s.GetOrder(orderId)).ReturnsAsync(orderResponse);
            _orderItemsBatchServiceMock.Setup(s => s.CreateOrderItems(requests)).ReturnsAsync((List<OrderItemResponse>?)null);

            var controller = CreateController();

            // Act
            var result = await controller.BatchOrderItems(orderId, requests);

            // Assert
            var problem = Assert.IsType<ObjectResult>(result);
            var details = Assert.IsType<ProblemDetails>(problem.Value);
            details.Detail.Should().Be("An error occurred while saving the order.");
        }

        #endregion
    }
    //public class OrderItemsControllerTest
    //{
    //    private readonly IOrderItemsAdderService _orderItemsAdderService;
    //    private readonly IOrderItemsDeleterService _orderItemsDeleterService;
    //    private readonly IOrderItemsGetterService _orderItemsGetterService;
    //    private readonly IOrderItemsUpdaterService _orderItemsUpdaterService;
    //    private readonly IOrdersGetterService _ordersGetterService;
    //    private readonly IOrderItemsBatchService _orderItemsBatchService;

    //    private readonly Mock<IOrderItemsAdderService> _orderItemsAdderServiceMock;
    //    private readonly Mock<IOrderItemsDeleterService> _orderItemsDeleterServiceMock;
    //    private readonly Mock<IOrderItemsGetterService> _orderItemsGetterServiceMock;
    //    private readonly Mock<IOrderItemsUpdaterService> _orderItemsUpdaterServiceMock;
    //    private readonly Mock<IOrdersGetterService> _ordersGetterServiceMock;
    //    private readonly Mock<IOrderItemsBatchService> _orderItemsBatchServiceMock;

    //    private readonly ILogger<OrderItemsController> _logger;
    //    private readonly Mock<ILogger<OrderItemsController>> _loggerMock;

    //    private readonly Fixture _fixture;

    //    public OrderItemsControllerTest()
    //    {
    //        _fixture = new Fixture();

    //        //Handle circular references 
    //        _fixture.Behaviors
    //            .OfType<ThrowingRecursionBehavior>()
    //            .ToList()
    //            .ForEach(b => _fixture.Behaviors.Remove(b));

    //        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

    //        _orderItemsAdderServiceMock = new Mock<IOrderItemsAdderService>();
    //        _orderItemsDeleterServiceMock = new Mock<IOrderItemsDeleterService>();
    //        _orderItemsGetterServiceMock = new Mock<IOrderItemsGetterService>();
    //        _orderItemsUpdaterServiceMock = new Mock<IOrderItemsUpdaterService>();
    //        _ordersGetterServiceMock = new Mock<IOrdersGetterService>();
    //        _orderItemsBatchServiceMock = new Mock<IOrderItemsBatchService>();


    //        _loggerMock = new Mock<ILogger<OrderItemsController>>();

    //        _orderItemsAdderService = _orderItemsAdderServiceMock.Object;
    //        _orderItemsGetterService = _orderItemsGetterServiceMock.Object;
    //        _orderItemsDeleterService = _orderItemsDeleterServiceMock.Object;
    //        _orderItemsUpdaterService = _orderItemsUpdaterServiceMock.Object;
    //        _orderItemsBatchService = _orderItemsBatchServiceMock.Object;
    //        _ordersGetterService = _ordersGetterServiceMock.Object;
    //        _logger = _loggerMock.Object;
    //    }

    //    #region GetOrderItemsFromOrderId 

    //    [Fact]
    //    public async Task GetOrderItemsFromOrderId_ShouldReturnOkWithOrderItemsList()
    //    {
    //        // Arrange
    //        var orderId = Guid.NewGuid();
    //        var orderItems = _fixture.CreateMany<OrderItemResponse>(5).ToList();

    //        _ordersGetterServiceMock
    //            .Setup(s => s.GetOrder(orderId))
    //             .ReturnsAsync(_fixture.Build<OrderResponse>().With(x => x.OrderId, orderId).Create());

    //        _orderItemsGetterServiceMock.Setup(temp => temp.GetOrderItemsFromOrderID(orderId)).ReturnsAsync(orderItems);

    //        var controller = new OrderItemsController(_logger, _orderItemsDeleterService, _orderItemsAdderService,
    //            _orderItemsGetterService, _orderItemsUpdaterService, _ordersGetterService, _orderItemsBatchService);

    //        // Act
    //        var result = await controller.GetOrderItemsFromOrderId(orderId);

    //        // Assert
    //        var okResult = Assert.IsType<OkObjectResult>(result.Result);
    //        var returnValue = Assert.IsAssignableFrom<ICollection<OrderItemResponse>>(okResult.Value);

    //        Assert.Equal(orderItems.Count, returnValue.Count);

    //        returnValue.Should().BeEquivalentTo(orderItems);
    //    }
    //    #endregion

    //    #region GetOrderItem
    //    [Fact]
    //    public async Task GetOrderItem_ShouldReturnOrderResponse()
    //    {
    //        // Arrange
    //        var orderId = Guid.NewGuid();
    //        var orderItemId = Guid.NewGuid();

    //        var orderResponse = _fixture.Build<OrderResponse>()
    //                                    .With(x => x.OrderId, orderId)
    //                                    .Create();

    //        var orderItemResponse = _fixture.Build<OrderItemResponse>()
    //                                .With(x => x.OrderId, orderId)
    //                                .With(x => x.OrderItemId, orderItemId)
    //                                .Create();
    //        _ordersGetterServiceMock
    //            .Setup(s => s.GetOrder(It.IsAny<Guid>()))
    //            .ReturnsAsync(orderResponse);

    //        _orderItemsGetterServiceMock
    //            .Setup(s => s.GetOrderItem(It.IsAny<Guid>(), It.IsAny<Guid>()))
    //            .ReturnsAsync(orderItemResponse);

    //        var controller = new OrderItemsController(_logger, _orderItemsDeleterService, _orderItemsAdderService,
    //             _orderItemsGetterService, _orderItemsUpdaterService, _ordersGetterService, _orderItemsBatchService);

    //        // Act
    //        var result = await controller.GetOrderItem(orderId, orderItemId);

    //        // Assert
    //        var okResult = Assert.IsType<OkObjectResult>(result.Result);
    //        var returnValue = Assert.IsType<OrderItemResponse>(okResult.Value);

    //        returnValue.Should().BeEquivalentTo(orderItemResponse);
    //    }
    //    #endregion

    //    #region PutOrderItem 

    //    [Fact]
    //    public async Task PutOrderItem_ShouldReturnCreatedAtActionGetOrderItem()
    //    {
    //        // Arrange
    //        var updateRequest = _fixture.Create<OrderItemUpdateRequest>();
    //        var orderItemResponse = _fixture.Create<OrderItemResponse>();
    //        var orderResponse = _fixture.Create<OrderResponse>();

    //        _ordersGetterServiceMock
    //            .Setup(s => s.GetOrder(It.IsAny<Guid>()))
    //            .ReturnsAsync(orderResponse);

    //        _orderItemsGetterServiceMock.Setup(s => s.GetOrderItem(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(orderItemResponse);

    //        _orderItemsUpdaterServiceMock
    //            .Setup(temp => temp.UpdateOrderItem(It.IsAny<Guid>(), It.IsAny<OrderItemUpdateRequest>()))
    //            .ReturnsAsync(orderItemResponse);

    //        var controller = new OrderItemsController(_logger, _orderItemsDeleterService, _orderItemsAdderService,
    //            _orderItemsGetterService, _orderItemsUpdaterService, _ordersGetterService, _orderItemsBatchService);

    //        // Act
    //        var result = await controller.PutOrderItem(It.IsAny<Guid>(), updateRequest.OrderItemId, updateRequest);

    //        // Assert
    //        var createdAtAction = Assert.IsType<CreatedAtActionResult>(result);
    //        Assert.Equal(nameof(OrderItemsController.GetOrderItem), createdAtAction.ActionName);
    //        Assert.IsType<OrderItemResponse>(createdAtAction.Value);

    //        var actualResponse = createdAtAction.Value as OrderItemResponse;

    //        actualResponse.Should().BeEquivalentTo(orderItemResponse);
    //    }

    //    #endregion


    //    #region PostOrderItem

    //    [Fact]
    //    public async Task PostOrderItem_ShouldReturnCreatedAtAction_WithCreatedOrderItem()
    //    {
    //        // Arrange
    //        var addRequest = _fixture.Create<OrderItemAddRequest>();
    //        var orderResponse = _fixture.Build<OrderResponse>()
    //                            .With(o => o.OrderId, addRequest.OrderId)
    //                            .Create();
    //        var orderItemResponse = _fixture.Build<OrderItemResponse>()
    //                            .With(o => o.OrderId, addRequest.OrderId)
    //                            .Create();

    //        _ordersGetterServiceMock
    //            .Setup(temp => temp.GetOrder(It.Is<Guid>(id => id == addRequest.OrderId)))
    //            .ReturnsAsync(orderResponse);

    //        _orderItemsAdderServiceMock
    //            .Setup(temp => temp.AddOrderItem(It.IsAny<OrderItemAddRequest>()))
    //            .ReturnsAsync(orderItemResponse);

    //        var controller = new OrderItemsController(_logger, _orderItemsDeleterService, _orderItemsAdderService,
    //            _orderItemsGetterService, _orderItemsUpdaterService, _ordersGetterService, _orderItemsBatchService);

    //        // Act
    //        var result = await controller.PostOrderItem(orderResponse.OrderId, addRequest);

    //        // Assert
    //        var createdAtAction = Assert.IsType<CreatedAtActionResult>(result);
    //        Assert.Equal(nameof(OrderItemsController.GetOrderItem), createdAtAction.ActionName);
    //        Assert.IsType<OrderItemResponse>(createdAtAction.Value);

    //        var expected = orderItemResponse;
    //        var actual = createdAtAction.Value as OrderItemResponse;

    //        actual.Should().BeEquivalentTo(expected);
    //    }

    //    #endregion

    //    #region DeleteOrderItem
    //    [Fact]
    //    public async Task DeleteOrderItem_ShouldReturnNoContent()
    //    {
    //        // Arrange
    //        var orderId = Guid.NewGuid();
    //        var orderItemId = Guid.NewGuid();

    //        var orderItemResponse = _fixture.Build<OrderItemResponse>()
    //                                        .With(x => x.OrderId, orderId)
    //                                        .With(x => x.OrderItemId, orderItemId)
    //                                        .Create();

    //        _orderItemsGetterServiceMock
    //            .Setup(s => s.GetOrderItem(orderId, orderItemId))
    //            .ReturnsAsync(orderItemResponse);

    //        _orderItemsDeleterServiceMock
    //            .Setup(s => s.DeleteOrderItem(orderId, orderItemId))
    //            .ReturnsAsync(true);

    //        var controller = new OrderItemsController(_logger, _orderItemsDeleterService, _orderItemsAdderService,
    //            _orderItemsGetterService, _orderItemsUpdaterService, _ordersGetterService, _orderItemsBatchService);

    //        // Act
    //        var result = await controller.DeleteOrderItem(orderId, orderItemId);

    //        // Assert
    //        Assert.IsType<NoContentResult>(result);
    //    }

    //    #endregion

    //    #region BatchOrderItem

    //    [Fact]
    //    public async Task BatchOrderItems_ShouldReturnCreatedAtAction_WithCreatedOrderItems()
    //    {
    //        // Arrange
    //        var orderId = Guid.NewGuid();

    //        OrderResponse? orderResponse = _fixture
    //            .Build<OrderResponse>()
    //            .With(x => x.OrderId, orderId)
    //            .Create();

    //        var orderResponses = _fixture
    //            .Build<OrderItemResponse>()
    //            .With(x => x.OrderId, orderId)
    //            .CreateMany(2)
    //            .ToList();

    //        _ordersGetterServiceMock.Setup(temp => temp.GetOrder(It.IsAny<Guid>())).ReturnsAsync(orderResponse);

    //        _orderItemsBatchServiceMock
    //            .Setup(temp => temp.CreateOrderItems(It.IsAny<List<OrderItemAddRequest>>()))
    //            .ReturnsAsync(orderResponses);

    //        var controller = new OrderItemsController(_logger, _orderItemsDeleterService, _orderItemsAdderService,
    //            _orderItemsGetterService, _orderItemsUpdaterService, _ordersGetterService, _orderItemsBatchService);

    //        // Act
    //        var result = await controller.BatchOrderItems(orderId, new List<OrderItemAddRequest>() { new OrderItemAddRequest() { OrderId = orderId }, new OrderItemAddRequest() { OrderId = orderId } });

    //        // Assert
    //        var createdAtAction = Assert.IsType<CreatedAtActionResult>(result);
    //        Assert.Equal(nameof(OrderItemsController.GetOrderItemsFromOrderId), createdAtAction.ActionName);
    //        Assert.IsType<List<OrderItemResponse>>(createdAtAction.Value);

    //        var returnedOrder = createdAtAction.Value as List<OrderItemResponse>;

    //        returnedOrder.Should().BeEquivalentTo(orderResponses);

    //    }
    //    #endregion
    //}
}
