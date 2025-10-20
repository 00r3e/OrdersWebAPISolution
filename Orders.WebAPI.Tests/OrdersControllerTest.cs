using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Azure;
using Castle.Core.Logging;
using Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Orders.WebAPI.Controllers;
using RepositoryContracts;
using ServiceContracts.DTO.CustomersDTO;
using ServiceContracts.DTO.OrderDTO;
using ServiceContracts.ICustomersServices;
using ServiceContracts.IOrdersServices;
using Services;

namespace Orders.WebAPI.Tests
{

    public class OrdersControllerTest
    {
        private readonly Mock<IOrdersAdderService> _ordersAdderServiceMock;
        private readonly Mock<IOrdersDeleterService> _ordersDeleterServiceMock;
        private readonly Mock<IOrdersGetterService> _ordersGetterServiceMock;
        private readonly Mock<IOrdersUpdaterService> _ordersUpdaterServiceMock;
        private readonly Mock<IOrdersBatchService> _ordersBatchServiceMock;
        private readonly Mock<ICustomersGetterService> _customersGetterServiceMock;
        private readonly Mock<ILogger<OrdersController>> _loggerMock;

        private readonly Fixture _fixture;

        public OrdersControllerTest()
        {
            _fixture = new Fixture();

            // Handle circular references
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _ordersAdderServiceMock = new Mock<IOrdersAdderService>();
            _ordersDeleterServiceMock = new Mock<IOrdersDeleterService>();
            _ordersGetterServiceMock = new Mock<IOrdersGetterService>();
            _ordersUpdaterServiceMock = new Mock<IOrdersUpdaterService>();
            _ordersBatchServiceMock = new Mock<IOrdersBatchService>();
            _customersGetterServiceMock = new Mock<ICustomersGetterService>();
            _loggerMock = new Mock<ILogger<OrdersController>>();
        }

        private OrdersController CreateController()
        {
            return new OrdersController(
                _ordersAdderServiceMock.Object,
                _ordersDeleterServiceMock.Object,
                _ordersGetterServiceMock.Object,
                _ordersUpdaterServiceMock.Object,
                _ordersBatchServiceMock.Object,
                _customersGetterServiceMock.Object,
                _loggerMock.Object
            );
        }

        #region GET /api/orders

        [Fact]
        public async Task GetOrders_ShouldReturnOk_WithOrdersList()
        {
            // Arrange
            var orders = _fixture.CreateMany<OrderResponse>(5).ToList();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrders())
                .ReturnsAsync(orders);

            var controller = CreateController();

            // Act
            var result = await controller.GetOrders();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<OrderResponse>>(okResult.Value);
            returnValue.Should().BeEquivalentTo(orders);
        }

        #endregion

        #region GET /api/orders/{orderId}

        [Fact]
        public async Task GetOrder_ShouldReturnOk_WithOrder()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderResponse = _fixture.Build<OrderResponse>()
                                        .With(x => x.OrderId, orderId)
                                        .Create();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync(orderResponse);

            var controller = CreateController();

            // Act
            var result = await controller.GetOrder(orderId);

            // Assert
            var okResult = Assert.IsType<ActionResult<OrderResponse>>(result);
            var value = result.Value;
            value.Should().BeEquivalentTo(orderResponse);
        }

        #endregion

        #region PUT /api/orders/{orderId}

        [Fact]
        public async Task PutOrder_ShouldReturnCreatedAtActionResult_WithUpdatedOrder()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var updateRequest = _fixture.Build<OrderUpdateRequest>()
                                        .With(o => o.OrderId, orderId)
                                        .Create();

            var orderResponse = _fixture.Build<OrderResponse>()
                                        .With(o => o.OrderId, orderId)
                                        .Create();

            var customerResponse = _fixture.Create<CustomerResponse>();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync(orderResponse);

            _customersGetterServiceMock
                .Setup(s => s.GetCustomer(updateRequest.CustomerId))
                .ReturnsAsync(customerResponse);

            _ordersUpdaterServiceMock
                .Setup(s => s.UpdateOrder(updateRequest))
                .ReturnsAsync(orderResponse);

            var controller = CreateController();

            // Act
            var result = await controller.PutOrder(orderId, updateRequest);

            // Assert
            var createdAtAction = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(OrdersController.GetOrder), createdAtAction.ActionName);
            var actualResponse = Assert.IsType<OrderResponse>(createdAtAction.Value);
            actualResponse.Should().BeEquivalentTo(orderResponse);
        }

        #endregion

        #region POST /api/orders

        [Fact]
        public async Task PostOrder_ShouldReturnCreatedAtActionResult_WithCreatedOrder()
        {
            // Arrange
            var addRequest = _fixture.Create<OrderAddRequest>();
            var orderResponse = _fixture.Build<OrderResponse>()
                                        .With(o => o.OrderId, Guid.NewGuid())
                                        .Create();

            _ordersAdderServiceMock
                .Setup(s => s.AddOrder(addRequest))
                .ReturnsAsync(orderResponse);

            var controller = CreateController();

            // Act
            var result = await controller.PostOrder(addRequest);

            // Assert
            var createdAtAction = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(OrdersController.GetOrder), createdAtAction.ActionName);
            var actualResponse = Assert.IsType<OrderResponse>(createdAtAction.Value);
            actualResponse.Should().BeEquivalentTo(orderResponse);
        }

        #endregion

        #region DELETE /api/orders/{orderId}

        [Fact]
        public async Task DeleteOrder_ShouldReturnNoContentResult()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderResponse = _fixture.Build<OrderResponse>()
                                        .With(x => x.OrderId, orderId)
                                        .Create();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync(orderResponse);

            _ordersDeleterServiceMock
                .Setup(s => s.DeleteOrder(orderId))
                .ReturnsAsync(true);

            var controller = CreateController();

            // Act
            var result = await controller.DeleteOrder(orderId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        #endregion

        #region POST /api/orders/batch

        [Fact]
        public async Task BatchOrders_ShouldReturnCreatedAtActionResult_WithCreatedOrdersList()
        {
            // Arrange
            var orderRequests = _fixture.CreateMany<OrderAddRequest>(3).ToList();
            var orderResponses = _fixture.CreateMany<OrderResponse>(3).ToList();

            _ordersBatchServiceMock
                .Setup(s => s.CreateOrders(orderRequests))
                .ReturnsAsync(orderResponses);

            var controller = CreateController();

            // Act
            var result = await controller.BatchOrders(orderRequests);

            // Assert
            var createdAtAction = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(OrdersController.GetOrders), createdAtAction.ActionName);
            var actualResponse = Assert.IsAssignableFrom<IEnumerable<OrderResponse>>(createdAtAction.Value);
            actualResponse.Should().BeEquivalentTo(orderResponses);
        }

        #endregion

        #region GET /api/orders/{orderId} (negative)

        [Fact]
        public async Task GetOrder_ShouldReturnNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync((OrderResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.GetOrder(orderId);

            // Assert
            var notFound = Assert.IsType<ActionResult<OrderResponse>>(result);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        #endregion

        #region PUT /api/orders/{orderId} (negative)

        [Fact]
        public async Task PutOrder_ShouldReturnBadRequest_WhenOrderIdMismatch()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var updateRequest = _fixture.Build<OrderUpdateRequest>()
                                        .With(o => o.OrderId, Guid.NewGuid())
                                        .Create();

            var controller = CreateController();

            // Act
            var result = await controller.PutOrder(orderId, updateRequest);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be("Order ID mismatch.");
        }

        [Fact]
        public async Task PutOrder_ShouldReturnNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var updateRequest = _fixture.Build<OrderUpdateRequest>()
                                        .With(o => o.OrderId, orderId)
                                        .Create();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync((OrderResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.PutOrder(orderId, updateRequest);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            notFound.Value.Should().Be("Order Id not found");
        }

        [Fact]
        public async Task PutOrder_ShouldReturnNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var updateRequest = _fixture.Build<OrderUpdateRequest>()
                                        .With(o => o.OrderId, orderId)
                                        .Create();
            var existingOrder = _fixture.Build<OrderResponse>()
                                        .With(o => o.OrderId, orderId)
                                        .Create();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync(existingOrder);
            _customersGetterServiceMock
                .Setup(s => s.GetCustomer(updateRequest.CustomerId))
                .ReturnsAsync((CustomerResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.PutOrder(orderId, updateRequest);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            notFound.Value.Should().Be("Customer Id not found");
        }

        [Fact]
        public async Task PutOrder_ShouldReturnProblem_WhenUpdateFails()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var updateRequest = _fixture.Build<OrderUpdateRequest>()
                                        .With(o => o.OrderId, orderId)
                                        .Create();
            var existingOrder = _fixture.Build<OrderResponse>()
                                        .With(o => o.OrderId, orderId)
                                        .Create();
            var existingCustomer = _fixture.Create<CustomerResponse>();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync(existingOrder);
            _customersGetterServiceMock
                .Setup(s => s.GetCustomer(updateRequest.CustomerId))
                .ReturnsAsync(existingCustomer);
            _ordersUpdaterServiceMock
                .Setup(s => s.UpdateOrder(updateRequest))
                .ReturnsAsync((OrderResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.PutOrder(orderId, updateRequest);

            // Assert
            var problem = Assert.IsType<ObjectResult>(result);
            var problemDetails = Assert.IsType<ProblemDetails>(problem.Value);
            problemDetails.Detail.Should().Be("Failed to update the order.");
            problem.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task PutOrder_ShouldReturnConflict_WhenDbConcurrencyExceptionOccurs()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var updateRequest = _fixture.Build<OrderUpdateRequest>()
                                        .With(o => o.OrderId, orderId)
                                        .Create();
            var existingOrder = _fixture.Build<OrderResponse>()
                                        .With(o => o.OrderId, orderId)
                                        .Create();
            var existingCustomer = _fixture.Create<CustomerResponse>();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync(existingOrder);
            _customersGetterServiceMock
                .Setup(s => s.GetCustomer(updateRequest.CustomerId))
                .ReturnsAsync(existingCustomer);
            _ordersUpdaterServiceMock
                .Setup(s => s.UpdateOrder(updateRequest))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            var controller = CreateController();

            // Act
            var result = await controller.PutOrder(orderId, updateRequest);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            conflict.Value.Should().Be("A concurrency conflict occurred.");
        }

        #endregion

        #region POST /api/orders (negative)

        [Fact]
        public async Task PostOrder_ShouldReturnBadRequest_WhenRequestIsNull()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.PostOrder(null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be("Order data is missing.");
        }

        [Fact]
        public async Task PostOrder_ShouldReturnProblem_WhenAddFails()
        {
            // Arrange
            var addRequest = _fixture.Create<OrderAddRequest>();

            _ordersAdderServiceMock
                .Setup(s => s.AddOrder(addRequest))
                .ReturnsAsync((OrderResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.PostOrder(addRequest);

            // Assert
            var problem = Assert.IsType<ObjectResult>(result);
            var problemDetails = Assert.IsType<ProblemDetails>(problem.Value);
            problemDetails.Detail.Should().Be("An error occurred while saving the order.");
            problem.StatusCode.Should().Be(500);
        }

        #endregion

        #region DELETE /api/orders/{orderId} (negative)

        [Fact]
        public async Task DeleteOrder_ShouldReturnNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync((OrderResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.DeleteOrder(orderId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteOrder_ShouldReturnProblem_WhenDeleteFails()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var existingOrder = _fixture.Build<OrderResponse>()
                                        .With(o => o.OrderId, orderId)
                                        .Create();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync(existingOrder);
            _ordersDeleterServiceMock
                .Setup(s => s.DeleteOrder(orderId))
                .ReturnsAsync(false);

            var controller = CreateController();

            // Act
            var result = await controller.DeleteOrder(orderId);

            // Assert
            var problem = Assert.IsType<ObjectResult>(result);
            var problemDetails = Assert.IsType<ProblemDetails>(problem.Value);
            problemDetails.Detail.Should().Be("Failed to delete the order.");
            problem.StatusCode.Should().Be(500);
        }

        #endregion

        #region POST /api/orders/batch (negative)

        [Fact]
        public async Task BatchOrders_ShouldReturnBadRequest_WhenListIsEmpty()
        {
            // Arrange
            var emptyList = new List<OrderAddRequest>();
            var controller = CreateController();

            // Act
            var result = await controller.BatchOrders(emptyList);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be("The list of order add requests cannot be empty");
        }

        #endregion
    }
    //public class OrdersControllerTest
    //{
    //    private readonly IOrdersAdderService _ordersAdderService;
    //    private readonly IOrdersDeleterService _ordersDeleterService;
    //    private readonly IOrdersGetterService _ordersGetterService;
    //    private readonly IOrdersUpdaterService _ordersUpdaterService;
    //    private readonly IOrdersBatchService _ordersBatchService;
    //    private readonly ICustomersGetterService _customersGetterService;

    //    private readonly Mock<IOrdersAdderService> _ordersAdderServiceMock;
    //    private readonly Mock<IOrdersDeleterService> _ordersDeleterServiceMock;
    //    private readonly Mock<IOrdersGetterService> _ordersGetterServiceMock;
    //    private readonly Mock<IOrdersUpdaterService> _ordersUpdaterServiceMock;
    //    private readonly Mock<IOrdersBatchService> _ordersBatchServiceMock;
    //    private readonly Mock<ICustomersGetterService> _customersGetterServiceMock;


    //    private readonly IOrderRepository _orderRepository;
    //    private readonly ILogger<OrdersController> _logger;
    //    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    //    private readonly Mock<ILogger<OrdersController>> _loggerMock;

    //    private readonly Fixture _fixture;

    //    private OrdersController CreateController() =>
    //                        new OrdersController(_ordersAdderService, _ordersDeleterService, _ordersGetterService,
    //                        _ordersUpdaterService, _ordersBatchService, _customersGetterService, _logger);

    //    public OrdersControllerTest()
    //    {
    //        _fixture = new Fixture();

    //        //Handle circular references 
    //        _fixture.Behaviors
    //            .OfType<ThrowingRecursionBehavior>()
    //            .ToList()
    //            .ForEach(b => _fixture.Behaviors.Remove(b));

    //        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

    //        _ordersAdderServiceMock = new Mock<IOrdersAdderService>();
    //        _ordersDeleterServiceMock = new Mock<IOrdersDeleterService>();
    //        _ordersGetterServiceMock = new Mock<IOrdersGetterService>();
    //        _ordersUpdaterServiceMock = new Mock<IOrdersUpdaterService>();
    //        _ordersBatchServiceMock = new Mock<IOrdersBatchService>();
    //        _customersGetterServiceMock = new Mock<ICustomersGetterService>();

    //        _orderRepositoryMock = new Mock<IOrderRepository>();
    //        _loggerMock = new Mock<ILogger<OrdersController>>();

    //        _ordersAdderService = _ordersAdderServiceMock.Object;
    //        _ordersGetterService = _ordersGetterServiceMock.Object;
    //        _ordersDeleterService = _ordersDeleterServiceMock.Object;
    //        _ordersUpdaterService = _ordersUpdaterServiceMock.Object;
    //        _ordersBatchService = _ordersBatchServiceMock.Object;
    //        _orderRepository = _orderRepositoryMock.Object;
    //        _customersGetterService = _customersGetterServiceMock.Object;
    //        _logger = _loggerMock.Object;
    //    }

    //    #region GetOrders 

    //    [Fact]
    //    public async Task GetOrders_WhenOrdersExist_ShouldReturnOkWithListOfOrderResponses()
    //    {
    //        //Arrange
    //        List<OrderResponse> orderResponses = _fixture.Create<List<OrderResponse>>();

    //        OrdersController ordersController = CreateController();

    //        _ordersGetterServiceMock.Setup(temp => temp.GetOrders()).ReturnsAsync(orderResponses);

    //        //Act
    //        ActionResult<IEnumerable<OrderResponse>> actionResult = await ordersController.GetOrders();

    //        //Assert
    //        OkObjectResult okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);

    //        var returnValue = Assert.IsAssignableFrom<IEnumerable<OrderResponse>>(okObjectResult.Value);
    //        Assert.Equal(orderResponses.Count, returnValue.Count());
    //    }

    //    #endregion

    //    #region GetOrder
    //    [Fact]

    //    public async Task GetOrder_WhenOrderExists_ShouldReturnOrderResponse()
    //    {
    //        //Arrange
    //        OrderResponse orderResponse = _fixture.Create<OrderResponse>();

    //        OrdersController controller = CreateController();

    //        _ordersGetterServiceMock.Setup(temp => temp.GetOrder(It.IsAny<Guid>())).ReturnsAsync(orderResponse);

    //        ActionResult<OrderResponse> actionResult = await controller.GetOrder(orderResponse.OrderId);

    //        // Assert
    //        var actual = actionResult.Value;

    //        actual.Should().BeEquivalentTo(orderResponse);
    //    }

    //    [Fact]

    //    public async Task GetOrder_WhenOrderDoesNotExist_ShouldReturnNotFound()
    //    {
    //        // Arrange
    //        Guid nonExistingOrderId = Guid.NewGuid();

    //        _ordersGetterServiceMock
    //            .Setup(repo => repo.GetOrder(nonExistingOrderId))
    //            .ReturnsAsync((OrderResponse?)null);

    //        OrdersController controller = CreateController();

    //        // Act
    //        ActionResult<OrderResponse> actionResult = await controller.GetOrder(nonExistingOrderId);

    //        // Assert
    //        Assert.IsType<NotFoundResult>(actionResult.Result);
    //    }

    //    #endregion

    //    #region PutOrder 

    //    [Fact]
    //    public async Task PutOrder_WhenValidUpdateRequest_ShouldReturnCreatedAtActionWithUpdatedOrder()
    //    {
    //        // Arrange
    //        OrderResponse existingOrderResponse = _fixture.Create<OrderResponse>();

    //        CustomerResponse customerResponse = _fixture.Create<CustomerResponse>();

    //        OrderUpdateRequest updateRequest = _fixture.Build<OrderUpdateRequest>().With(x => x.OrderId, existingOrderResponse.OrderId).Create();

    //        _ordersGetterServiceMock.Setup(temp => temp.GetOrder(existingOrderResponse.OrderId)).ReturnsAsync(existingOrderResponse);

    //        _ordersUpdaterServiceMock.Setup(repo => repo.UpdateOrder(It.IsAny<OrderUpdateRequest>())).ReturnsAsync(existingOrderResponse);

    //        _customersGetterServiceMock.Setup(temp => temp.GetCustomer(It.IsAny<Guid>())).ReturnsAsync(customerResponse);

    //        OrdersController controller = CreateController();

    //        // Act
    //        IActionResult result = await controller.PutOrder(existingOrderResponse.OrderId, updateRequest);

    //        // Assert
    //        CreatedAtActionResult createdAtAction = Assert.IsType<CreatedAtActionResult>(result);

    //        Assert.Equal(nameof(OrdersController.GetOrder), createdAtAction.ActionName);
    //        Assert.IsType<OrderResponse>(createdAtAction.Value);

    //        var returnedOrder = createdAtAction.Value as OrderResponse;

    //        returnedOrder.Should().BeEquivalentTo(existingOrderResponse);
    //    }

    //    [Fact]
    //    public async Task PutOrder_WhenOrderIdMismatch_ShouldReturnBadRequest()
    //    {
    //        // Arrange
    //        var routeOrderId = Guid.NewGuid();
    //        var differentOrderId = Guid.NewGuid();

    //        var updateRequest = _fixture.Build<OrderUpdateRequest>().With(x => x.OrderId, differentOrderId).Create();

    //        OrdersController controller = CreateController();

    //        // Act
    //        var result = await controller.PutOrder(routeOrderId, updateRequest);

    //        // Assert
    //        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    //        Assert.Equal("Order ID mismatch.", badRequestResult.Value);
    //    }

    //    #endregion

    //    #region PostOrder
    //    [Fact]
    //    public async Task PostOrder_WhenOrderIsValid_ShouldReturnCreatedAtActionWithOrderResponse()
    //    {
    //        // Arrange
    //        var orderResponse = _fixture.Create<OrderResponse>();
    //        var generatedOrderId = Guid.NewGuid();

    //        _ordersAdderServiceMock
    //            .Setup(temp => temp.AddOrder(It.IsAny<OrderAddRequest>()))
    //            .ReturnsAsync(orderResponse);

    //        OrdersController controller = CreateController();

    //        // Act
    //        var result = await controller.PostOrder(new OrderAddRequest());

    //        // Assert
    //        var createdAtAction = Assert.IsType<CreatedAtActionResult>(result);
    //        Assert.Equal(nameof(OrdersController.GetOrder), createdAtAction.ActionName);
    //        Assert.IsType<OrderResponse>(createdAtAction.Value);

    //        var returnedOrder = createdAtAction.Value as OrderResponse;

    //        returnedOrder.Should().BeEquivalentTo(orderResponse);
    //    }
    //    #endregion

    //    #region Delete
    //    [Fact]
    //    public async Task DeleteOrder_WhenOrderExists_ShouldReturnNoContent()
    //    {
    //        // Arrange
    //        var orderId = Guid.NewGuid();
    //        var existingOrder = _fixture.Create<OrderResponse>();

    //        _ordersGetterServiceMock.Setup(temp => temp.GetOrder(orderId)).ReturnsAsync(existingOrder);

    //        _ordersDeleterServiceMock.Setup(temp => temp.DeleteOrder(orderId)).ReturnsAsync(true);

    //        OrdersController controller = CreateController();

    //        // Act
    //        var result = await controller.DeleteOrder(orderId);

    //        // Assert
    //        Assert.IsType<NoContentResult>(result);
    //    }


    //    #endregion

    //    #region BatchOrder
    //    [Fact]
    //    public async Task BatchOrders_WhenValidRequests_ShouldReturnCreatedAtActionWithOrderList()
    //    {
    //        // Arrange
    //        var orderResponses = _fixture.CreateMany<OrderResponse>(2).ToList();


    //        _ordersBatchServiceMock
    //            .Setup(temp => temp.CreateOrders(It.IsAny<List<OrderAddRequest>>()))
    //            .ReturnsAsync(orderResponses);

    //        OrdersController controller = CreateController();

    //        // Act
    //        var result = await controller.BatchOrders(new List<OrderAddRequest>() { new OrderAddRequest(), new OrderAddRequest() });

    //        // Assert
    //        var createdAtAction = Assert.IsType<CreatedAtActionResult>(result);
    //        Assert.Equal(nameof(OrdersController.GetOrders), createdAtAction.ActionName);
    //        Assert.IsType<List<OrderResponse>>(createdAtAction.Value);

    //        var returnedOrder = createdAtAction.Value as List<OrderResponse>;

    //        returnedOrder.Should().BeEquivalentTo(orderResponses);
    //    }
    //    #endregion

    //}
}
