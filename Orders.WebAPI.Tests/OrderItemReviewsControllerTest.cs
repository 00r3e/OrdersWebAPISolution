using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Orders.WebAPI.Controllers;
using ServiceContracts.DTO.OrderDTO;
using ServiceContracts.DTO.OrderItemDTO;
using ServiceContracts.DTO.OrderItemReviewDTO;
using ServiceContracts.IOrderItemReviewsServices;
using ServiceContracts.IOrderItemsServices;
using ServiceContracts.IOrdersServices;

namespace Orders.WebAPI.Tests
{
    public class OrderItemReviewsControllerTest
    {
        private readonly Mock<IOrderItemReviewsAdderService> _orderItemReviewsAdderServiceMock;
        private readonly Mock<IOrderItemReviewsDeleterService> _orderItemReviewsDeleterServiceMock;
        private readonly Mock<IOrderItemReviewsGetterService> _orderItemReviewsGetterServiceMock;
        private readonly Mock<IOrderItemReviewsUpdaterService> _orderItemReviewsUpdaterServiceMock;
        private readonly Mock<IOrderItemsGetterService> _orderItemsGetterServiceMock;
        private readonly Mock<IOrdersGetterService> _ordersGetterServiceMock;
        private readonly Mock<ILogger<OrderItemReviewsController>> _loggerMock;

        private readonly Fixture _fixture;

        public OrderItemReviewsControllerTest()
        {
            _fixture = new Fixture();

            // Handle recursion
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _orderItemReviewsAdderServiceMock = new Mock<IOrderItemReviewsAdderService>();
            _orderItemReviewsDeleterServiceMock = new Mock<IOrderItemReviewsDeleterService>();
            _orderItemReviewsGetterServiceMock = new Mock<IOrderItemReviewsGetterService>();
            _orderItemReviewsUpdaterServiceMock = new Mock<IOrderItemReviewsUpdaterService>();
            _orderItemsGetterServiceMock = new Mock<IOrderItemsGetterService>();
            _ordersGetterServiceMock = new Mock<IOrdersGetterService>();
            _loggerMock = new Mock<ILogger<OrderItemReviewsController>>();
        }

        private OrderItemReviewsController CreateController()
        {
            return new OrderItemReviewsController(
                _loggerMock.Object,
                _orderItemReviewsDeleterServiceMock.Object,
                _orderItemReviewsAdderServiceMock.Object,
                _orderItemReviewsGetterServiceMock.Object,
                _orderItemReviewsUpdaterServiceMock.Object,
                _orderItemsGetterServiceMock.Object,
                _ordersGetterServiceMock.Object
            );
        }

        #region GET /api/orders/{orderId}/items/{orderItemId}/review

        [Fact]
        public async Task GetOrderItemReview_ShouldReturnOk_WithReview()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderItemId = Guid.NewGuid();
            var orderResponse = _fixture.Create<OrderResponse>();
            var reviewResponse = _fixture.Create<OrderItemReviewResponse>();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync(orderResponse);

            _orderItemReviewsGetterServiceMock
                .Setup(s => s.GetOrderItemReview(orderId, orderItemId))
                .ReturnsAsync(reviewResponse);

            var controller = CreateController();

            // Act
            var result = await controller.GetOrderItemReview(orderId, orderItemId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualValue = Assert.IsType<OrderItemReviewResponse>(okResult.Value);
            actualValue.Should().BeEquivalentTo(reviewResponse);
        }

        #endregion

        #region POST /api/orders/{orderId}/items/{orderItemId}/review

        [Fact]
        public async Task CreateReview_ShouldReturnCreatedAtAction_WithCreatedReview()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderItemId = Guid.NewGuid();

            var orderResponse = _fixture.Build<OrderResponse>()
                .With(o => o.OrderId, orderId)
                .Create();

            var orderItemResponse = _fixture.Create<OrderItemResponse>();
            var addRequest = _fixture.Build<OrderItemReviewAddRequest>()
                .With(r => r.OrderItemId, orderItemId)
                .Create();

            var createdReview = _fixture.Build<OrderItemReviewResponse>()
                .With(r => r.OrderItemId, orderItemId)
                .Create();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync(orderResponse);

            _orderItemsGetterServiceMock
                .Setup(s => s.GetOrderItem(orderId, orderItemId))
                .ReturnsAsync(orderItemResponse);

            _orderItemReviewsGetterServiceMock
                .Setup(s => s.GetOrderItemReview(orderId, orderItemId))
                .ReturnsAsync((OrderItemReviewResponse?)null);

            _orderItemReviewsAdderServiceMock
                .Setup(s => s.AddOrderItemReview(orderResponse.CustomerId, addRequest))
                .ReturnsAsync(createdReview);

            var controller = CreateController();

            // Act
            var result = await controller.CreateReview(orderId, orderItemId, addRequest);

            // Assert
            var createdAtAction = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(OrderItemReviewsController.GetOrderItemReview), createdAtAction.ActionName);
            var value = Assert.IsType<OrderItemReviewResponse>(createdAtAction.Value);
            value.Should().BeEquivalentTo(createdReview);
        }

        #endregion

        #region PUT /api/orders/{orderId}/items/{orderItemId}/review

        [Fact]
        public async Task UpdateOrderItemReview_ShouldReturnOk_WithUpdatedReview()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderItemId = Guid.NewGuid();

            var orderResponse = _fixture.Create<OrderResponse>();
            var orderItemResponse = _fixture.Create<OrderItemResponse>();
            var existingReview = _fixture.Create<OrderItemReviewResponse>();
            var updateRequest = _fixture.Build<OrderItemReviewUpdateRequest>()
                .With(r => r.OrderItemId, orderItemId)
                .Create();

            var updatedReview = _fixture.Create<OrderItemReviewResponse>();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync(orderResponse);

            _orderItemsGetterServiceMock
                .Setup(s => s.GetOrderItem(orderId, orderItemId))
                .ReturnsAsync(orderItemResponse);

            _orderItemReviewsGetterServiceMock
                .Setup(s => s.GetOrderItemReview(orderId, orderItemId))
                .ReturnsAsync(existingReview);

            _orderItemReviewsUpdaterServiceMock
                .Setup(s => s.UpdateOrderItemReview(orderResponse.CustomerId, updateRequest))
                .ReturnsAsync(updatedReview);

            var controller = CreateController();

            // Act
            var result = await controller.UpdateOrderItemReview(orderId, orderItemId, updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualReview = Assert.IsType<OrderItemReviewResponse>(okResult.Value);
            actualReview.Should().BeEquivalentTo(updatedReview);
        }

        #endregion

        #region DELETE /api/orders/{orderId}/items/{orderItemId}/review

        [Fact]
        public async Task DeleteReview_ShouldReturnNoContent_WhenDeletedSuccessfully()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderItemId = Guid.NewGuid();

            var orderResponse = _fixture.Create<OrderResponse>();
            var orderItemResponse = _fixture.Create<OrderItemResponse>();
            var reviewResponse = _fixture.Create<OrderItemReviewResponse>();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync(orderResponse);

            _orderItemsGetterServiceMock
                .Setup(s => s.GetOrderItem(orderId, orderItemId))
                .ReturnsAsync(orderItemResponse);

            _orderItemReviewsGetterServiceMock
                .Setup(s => s.GetOrderItemReview(orderId, orderItemId))
                .ReturnsAsync(reviewResponse);

            _orderItemReviewsDeleterServiceMock
                .Setup(s => s.DeleteOrderItemReview(orderId, orderItemId))
                .ReturnsAsync(true);

            var controller = CreateController();

            // Act
            var result = await controller.DeleteReview(orderId, orderItemId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        #endregion

        #region NEGATIVE CASES — GET /api/orders/{orderId}/items/{orderItemId}/review

        [Fact]
        public async Task GetOrderItemReview_ShouldReturnBadRequest_WhenIdsAreInvalid()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.GetOrderItemReview(Guid.Empty, Guid.Empty);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            badRequest.Value.Should().Be("Invalid orderId or orderItemId");
        }

        [Fact]
        public async Task GetOrderItemReview_ShouldReturnNotFound_WhenOrderDoesNotExist()
        {
            var orderId = Guid.NewGuid();
            var itemId = Guid.NewGuid();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync((OrderResponse?)null);

            var controller = CreateController();

            var result = await controller.GetOrderItemReview(orderId, itemId);

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFound.Value.Should().Be("Order not found");
        }

        [Fact]
        public async Task GetOrderItemReview_ShouldReturnNotFound_WhenReviewNotFound()
        {
            var orderId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var order = _fixture.Create<OrderResponse>();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync(order);

            _orderItemReviewsGetterServiceMock
                .Setup(s => s.GetOrderItemReview(orderId, itemId))
                .ReturnsAsync((OrderItemReviewResponse?)null);

            var controller = CreateController();

            var result = await controller.GetOrderItemReview(orderId, itemId);

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFound.Value.Should().Be("Review not found");
        }

        #endregion


        #region NEGATIVE CASES — POST /api/orders/{orderId}/items/{orderItemId}/review

        [Fact]
        public async Task CreateReview_ShouldReturnBadRequest_WhenBodyIsNull()
        {
            var controller = CreateController();

            var result = await controller.CreateReview(Guid.NewGuid(), Guid.NewGuid(), null!);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            badRequest.Value.Should().Be("Review data is missing");
        }

        [Fact]
        public async Task CreateReview_ShouldReturnBadRequest_WhenOrderItemIdMismatch()
        {
            var orderId = Guid.NewGuid();
            var orderItemId = Guid.NewGuid();
            var badRequestBody = _fixture.Build<OrderItemReviewAddRequest>()
                .With(r => r.OrderItemId, Guid.NewGuid())
                .Create();

            var controller = CreateController();

            var result = await controller.CreateReview(orderId, orderItemId, badRequestBody);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            badRequest.Value.Should().Be("OrderItemId in body does not match route");
        }

        [Fact]
        public async Task CreateReview_ShouldReturnNotFound_WhenOrderNotFound()
        {
            var orderId = Guid.NewGuid();
            var orderItemId = Guid.NewGuid();
            var request = _fixture.Build<OrderItemReviewAddRequest>()
                .With(r => r.OrderItemId, orderItemId)
                .Create();

            _ordersGetterServiceMock
                .Setup(s => s.GetOrder(orderId))
                .ReturnsAsync((OrderResponse?)null);

            var controller = CreateController();

            var result = await controller.CreateReview(orderId, orderItemId, request);

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFound.Value.Should().Be("Order not found");
        }

        [Fact]
        public async Task CreateReview_ShouldReturnNotFound_WhenOrderItemNotFound()
        {
            var orderId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var order = _fixture.Create<OrderResponse>();
            var request = _fixture.Build<OrderItemReviewAddRequest>().With(r => r.OrderItemId, itemId).Create();

            _ordersGetterServiceMock.Setup(s => s.GetOrder(orderId)).ReturnsAsync(order);
            _orderItemsGetterServiceMock.Setup(s => s.GetOrderItem(orderId, itemId)).ReturnsAsync((OrderItemResponse?)null);

            var controller = CreateController();

            var result = await controller.CreateReview(orderId, itemId, request);

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFound.Value.Should().Be("Review item not found");
        }

        [Fact]
        public async Task CreateReview_ShouldReturnConflict_WhenReviewAlreadyExists()
        {
            var orderId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var order = _fixture.Create<OrderResponse>();
            var orderItem = _fixture.Create<OrderItemResponse>();
            var existingReview = _fixture.Create<OrderItemReviewResponse>();
            var request = _fixture.Build<OrderItemReviewAddRequest>().With(r => r.OrderItemId, itemId).Create();

            _ordersGetterServiceMock.Setup(s => s.GetOrder(orderId)).ReturnsAsync(order);
            _orderItemsGetterServiceMock.Setup(s => s.GetOrderItem(orderId, itemId)).ReturnsAsync(orderItem);
            _orderItemReviewsGetterServiceMock.Setup(s => s.GetOrderItemReview(orderId, itemId)).ReturnsAsync(existingReview);

            var controller = CreateController();

            var result = await controller.CreateReview(orderId, itemId, request);

            var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
            conflict.Value.Should().Be("Review already exists for this order item");
        }

        [Fact]
        public async Task CreateReview_ShouldReturnProblem_WhenCreationFails()
        {
            var orderId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var order = _fixture.Create<OrderResponse>();
            var orderItem = _fixture.Create<OrderItemResponse>();
            var request = _fixture.Build<OrderItemReviewAddRequest>().With(r => r.OrderItemId, itemId).Create();

            _ordersGetterServiceMock.Setup(s => s.GetOrder(orderId)).ReturnsAsync(order);
            _orderItemsGetterServiceMock.Setup(s => s.GetOrderItem(orderId, itemId)).ReturnsAsync(orderItem);
            _orderItemReviewsGetterServiceMock.Setup(s => s.GetOrderItemReview(orderId, itemId)).ReturnsAsync((OrderItemReviewResponse?)null);
            _orderItemReviewsAdderServiceMock.Setup(s => s.AddOrderItemReview(order.CustomerId, request)).ReturnsAsync((OrderItemReviewResponse?)null);

            var controller = CreateController();

            var result = await controller.CreateReview(orderId, itemId, request);

            var problem = Assert.IsType<ObjectResult>(result.Result);
            var details = Assert.IsType<ProblemDetails>(problem.Value);
            details.Detail.Should().Be("Error creating review");
            problem.StatusCode.Should().Be(500);
        }

        #endregion


        #region NEGATIVE CASES — PUT /api/orders/{orderId}/items/{orderItemId}/review

        [Fact]
        public async Task UpdateOrderItemReview_ShouldReturnBadRequest_WhenBodyIsNull()
        {
            var controller = CreateController();

            var result = await controller.UpdateOrderItemReview(Guid.NewGuid(), Guid.NewGuid(), null!);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            badRequest.Value.Should().Be("Update data is missing");
        }

        [Fact]
        public async Task UpdateOrderItemReview_ShouldReturnBadRequest_WhenOrderItemIdMismatch()
        {
            var orderId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var request = _fixture.Build<OrderItemReviewUpdateRequest>()
                .With(r => r.OrderItemId, Guid.NewGuid())
                .Create();

            var controller = CreateController();

            var result = await controller.UpdateOrderItemReview(orderId, itemId, request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            badRequest.Value.Should().Be("OrderItemId in body does not match route");
        }

        [Fact]
        public async Task UpdateOrderItemReview_ShouldReturnNotFound_WhenOrderNotFound()
        {
            var orderId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var request = _fixture.Build<OrderItemReviewUpdateRequest>().With(r => r.OrderItemId, itemId).Create();

            _ordersGetterServiceMock.Setup(s => s.GetOrder(orderId)).ReturnsAsync((OrderResponse?)null);

            var controller = CreateController();

            var result = await controller.UpdateOrderItemReview(orderId, itemId, request);

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFound.Value.Should().Be("Order not found");
        }

        [Fact]
        public async Task UpdateOrderItemReview_ShouldReturnNotFound_WhenOrderItemNotFound()
        {
            var orderId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var order = _fixture.Create<OrderResponse>();
            var request = _fixture.Build<OrderItemReviewUpdateRequest>().With(r => r.OrderItemId, itemId).Create();

            _ordersGetterServiceMock.Setup(s => s.GetOrder(orderId)).ReturnsAsync(order);
            _orderItemsGetterServiceMock.Setup(s => s.GetOrderItem(orderId, itemId)).ReturnsAsync((OrderItemResponse?)null);

            var controller = CreateController();

            var result = await controller.UpdateOrderItemReview(orderId, itemId, request);

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFound.Value.Should().Be("Order item not found");
        }

        [Fact]
        public async Task UpdateOrderItemReview_ShouldReturnNotFound_WhenReviewNotFound()
        {
            var orderId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var order = _fixture.Create<OrderResponse>();
            var item = _fixture.Create<OrderItemResponse>();
            var request = _fixture.Build<OrderItemReviewUpdateRequest>().With(r => r.OrderItemId, itemId).Create();

            _ordersGetterServiceMock.Setup(s => s.GetOrder(orderId)).ReturnsAsync(order);
            _orderItemsGetterServiceMock.Setup(s => s.GetOrderItem(orderId, itemId)).ReturnsAsync(item);
            _orderItemReviewsGetterServiceMock.Setup(s => s.GetOrderItemReview(orderId, itemId)).ReturnsAsync((OrderItemReviewResponse?)null);

            var controller = CreateController();

            var result = await controller.UpdateOrderItemReview(orderId, itemId, request);

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFound.Value.Should().Be("Order item review not found");
        }

        [Fact]
        public async Task UpdateOrderItemReview_ShouldReturnProblem_WhenUpdateFails()
        {
            var orderId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var order = _fixture.Create<OrderResponse>();
            var item = _fixture.Create<OrderItemResponse>();
            var review = _fixture.Create<OrderItemReviewResponse>();
            var request = _fixture.Build<OrderItemReviewUpdateRequest>().With(r => r.OrderItemId, itemId).Create();

            _ordersGetterServiceMock.Setup(s => s.GetOrder(orderId)).ReturnsAsync(order);
            _orderItemsGetterServiceMock.Setup(s => s.GetOrderItem(orderId, itemId)).ReturnsAsync(item);
            _orderItemReviewsGetterServiceMock.Setup(s => s.GetOrderItemReview(orderId, itemId)).ReturnsAsync(review);
            _orderItemReviewsUpdaterServiceMock.Setup(s => s.UpdateOrderItemReview(order.CustomerId, request)).ReturnsAsync((OrderItemReviewResponse?)null);

            var controller = CreateController();

            var result = await controller.UpdateOrderItemReview(orderId, itemId, request);

            var problem = Assert.IsType<ObjectResult>(result.Result);
            var details = Assert.IsType<ProblemDetails>(problem.Value);
            details.Detail.Should().Be("Error updating review");
            problem.StatusCode.Should().Be(500);
        }

        #endregion


        #region NEGATIVE CASES — DELETE /api/orders/{orderId}/items/{orderItemId}/review

        [Fact]
        public async Task DeleteReview_ShouldReturnNotFound_WhenOrderNotFound()
        {
            var orderId = Guid.NewGuid();
            var itemId = Guid.NewGuid();

            _ordersGetterServiceMock.Setup(s => s.GetOrder(orderId)).ReturnsAsync((OrderResponse?)null);

            var controller = CreateController();

            var result = await controller.DeleteReview(orderId, itemId);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            notFound.Value.Should().Be("Ordernot found");
        }

        [Fact]
        public async Task DeleteReview_ShouldReturnNotFound_WhenOrderItemNotFound()
        {
            var orderId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var order = _fixture.Create<OrderResponse>();

            _ordersGetterServiceMock.Setup(s => s.GetOrder(orderId)).ReturnsAsync(order);
            _orderItemsGetterServiceMock.Setup(s => s.GetOrderItem(orderId, itemId)).ReturnsAsync((OrderItemResponse?)null);

            var controller = CreateController();

            var result = await controller.DeleteReview(orderId, itemId);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            notFound.Value.Should().Be("Order item not found");
        }

        [Fact]
        public async Task DeleteReview_ShouldReturnNotFound_WhenReviewNotFound()
        {
            var orderId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var order = _fixture.Create<OrderResponse>();
            var item = _fixture.Create<OrderItemResponse>();

            _ordersGetterServiceMock.Setup(s => s.GetOrder(orderId)).ReturnsAsync(order);
            _orderItemsGetterServiceMock.Setup(s => s.GetOrderItem(orderId, itemId)).ReturnsAsync(item);
            _orderItemReviewsGetterServiceMock.Setup(s => s.GetOrderItemReview(orderId, itemId)).ReturnsAsync((OrderItemReviewResponse?)null);

            var controller = CreateController();

            var result = await controller.DeleteReview(orderId, itemId);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            notFound.Value.Should().Be("Order item review not found");
        }

        [Fact]
        public async Task DeleteReview_ShouldReturnProblem_WhenDeletionFails()
        {
            var orderId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var order = _fixture.Create<OrderResponse>();
            var item = _fixture.Create<OrderItemResponse>();
            var review = _fixture.Create<OrderItemReviewResponse>();

            _ordersGetterServiceMock.Setup(s => s.GetOrder(orderId)).ReturnsAsync(order);
            _orderItemsGetterServiceMock.Setup(s => s.GetOrderItem(orderId, itemId)).ReturnsAsync(item);
            _orderItemReviewsGetterServiceMock.Setup(s => s.GetOrderItemReview(orderId, itemId)).ReturnsAsync(review);
            _orderItemReviewsDeleterServiceMock.Setup(s => s.DeleteOrderItemReview(orderId, itemId)).ReturnsAsync(false);

            var controller = CreateController();

            var result = await controller.DeleteReview(orderId, itemId);

            var problem = Assert.IsType<ObjectResult>(result);
            var details = Assert.IsType<ProblemDetails>(problem.Value);
            details.Detail.Should().Be("Error deleting order item review");
            problem.StatusCode.Should().Be(500);
        }

        #endregion
    }

}
