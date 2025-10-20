using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Orders.WebAPI.Controllers;
using ServiceContracts.DTO.CustomersDTO;
using ServiceContracts.ICustomersServices;

namespace Orders.WebAPI.Tests
{
    public class CustomersControllerTest
    {
        private readonly Mock<ICustomersAdderService> _customersAdderServiceMock;
        private readonly Mock<ICustomersDeleterService> _customersDeleterServiceMock;
        private readonly Mock<ICustomersGetterService> _customersGetterServiceMock;
        private readonly Mock<ICustomersUpdaterService> _customersUpdaterServiceMock;
        private readonly Mock<ILogger<CustomersController>> _loggerMock;
        private readonly Fixture _fixture;

        public CustomersControllerTest()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _customersAdderServiceMock = new Mock<ICustomersAdderService>();
            _customersDeleterServiceMock = new Mock<ICustomersDeleterService>();
            _customersGetterServiceMock = new Mock<ICustomersGetterService>();
            _customersUpdaterServiceMock = new Mock<ICustomersUpdaterService>();
            _loggerMock = new Mock<ILogger<CustomersController>>();
        }

        private CustomersController CreateController() =>
            new CustomersController(
                _customersAdderServiceMock.Object,
                _customersDeleterServiceMock.Object,
                _customersGetterServiceMock.Object,
                _customersUpdaterServiceMock.Object,
                _loggerMock.Object);

        #region GET /api/customers

        [Fact]
        public async Task GetCustomers_ShouldReturnOk_WithListOfCustomers()
        {
            var customers = _fixture.CreateMany<CustomerResponse>(5).ToList();
            _customersGetterServiceMock.Setup(s => s.GetAllCustomers()).ReturnsAsync(customers);
            var controller = CreateController();

            var result = await controller.GetCustomers();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<CustomerResponse>>(ok.Value);
            value.Should().BeEquivalentTo(customers);
        }

        [Fact]
        public async Task GetCustomers_ShouldReturnOk_WhenEmptyList()
        {
            _customersGetterServiceMock.Setup(s => s.GetAllCustomers()).ReturnsAsync(new List<CustomerResponse>());
            var controller = CreateController();

            var result = await controller.GetCustomers();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<CustomerResponse>>(ok.Value);
            value.Should().BeEmpty();
        }

        #endregion

        #region GET /api/customers/{customerId}

        [Fact]
        public async Task GetCustomer_ShouldReturnCustomer_WhenFound()
        {
            var id = Guid.NewGuid();
            var expected = _fixture.Build<CustomerResponse>().With(c => c.CustomerId, id).Create();

            _customersGetterServiceMock.Setup(s => s.GetCustomer(id)).ReturnsAsync(expected);
            var controller = CreateController();

            var result = await controller.GetCustomer(id);

            result.Value.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetCustomer_ShouldReturnNotFound_WhenCustomerDoesNotExist()
        {
            var id = Guid.NewGuid();
            _customersGetterServiceMock.Setup(s => s.GetCustomer(id)).ReturnsAsync((CustomerResponse?)null);
            var controller = CreateController();

            var result = await controller.GetCustomer(id);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        #endregion

        #region PUT /api/customers/{customerId}

        [Fact]
        public async Task PutCustomer_ShouldReturnCreatedAtAction_WhenUpdatedSuccessfully()
        {
            var id = Guid.NewGuid();
            var updateRequest = _fixture.Build<CustomerUpdateRequest>().With(c => c.CustomerId, id).Create();
            var existing = _fixture.Build<CustomerResponse>().With(c => c.CustomerId, id).Create();
            var updated = _fixture.Build<CustomerResponse>().With(c => c.CustomerId, id).Create();

            _customersGetterServiceMock.Setup(s => s.GetCustomer(id)).ReturnsAsync(existing);
            _customersUpdaterServiceMock.Setup(s => s.UpdateCustomer(updateRequest)).ReturnsAsync(updated);

            var controller = CreateController();

            var result = await controller.PutCustomer(id, updateRequest);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            created.Value.Should().BeEquivalentTo(updated);
        }

        [Fact]
        public async Task PutCustomer_ShouldReturnBadRequest_WhenIdMismatch()
        {
            var updateRequest = _fixture.Create<CustomerUpdateRequest>();
            var controller = CreateController();

            var result = await controller.PutCustomer(Guid.NewGuid(), updateRequest);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be("Customer ID mismatch.");
        }

        [Fact]
        public async Task PutCustomer_ShouldReturnNotFound_WhenCustomerDoesNotExist()
        {
            var id = Guid.NewGuid();
            var request = _fixture.Build<CustomerUpdateRequest>().With(c => c.CustomerId, id).Create();
            _customersGetterServiceMock.Setup(s => s.GetCustomer(id)).ReturnsAsync((CustomerResponse?)null);

            var controller = CreateController();

            var result = await controller.PutCustomer(id, request);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PutCustomer_ShouldReturnProblem_WhenUpdateFails()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = _fixture.Build<CustomerUpdateRequest>()
                                  .With(c => c.CustomerId, id)
                                  .Create();
            var existing = _fixture.Build<CustomerResponse>()
                                   .With(c => c.CustomerId, id)
                                   .Create();

            _customersGetterServiceMock.Setup(s => s.GetCustomer(id)).ReturnsAsync(existing);
            _customersUpdaterServiceMock.Setup(s => s.UpdateCustomer(request))
                                        .ReturnsAsync((CustomerResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.PutCustomer(id, request);

            // Assert
            var problem = Assert.IsType<ObjectResult>(result);
            var problemDetails = Assert.IsType<ProblemDetails>(problem.Value);
            problemDetails.Detail.Should().Be("Failed to update the customer.");
            problem.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task PutCustomer_ShouldReturnConflict_WhenConcurrencyExceptionOccurs()
        {
            var id = Guid.NewGuid();
            var request = _fixture.Build<CustomerUpdateRequest>().With(c => c.CustomerId, id).Create();
            var existing = _fixture.Build<CustomerResponse>().With(c => c.CustomerId, id).Create();

            _customersGetterServiceMock.Setup(s => s.GetCustomer(id)).ReturnsAsync(existing);
            _customersUpdaterServiceMock.Setup(s => s.UpdateCustomer(request)).ThrowsAsync(new DbUpdateConcurrencyException());

            var controller = CreateController();

            var result = await controller.PutCustomer(id, request);

            var conflict = Assert.IsType<ConflictObjectResult>(result);
            conflict.Value.Should().Be("A concurrency conflict occurred.");
        }

        #endregion

        #region POST /api/customers

        [Fact]
        public async Task PostCustomer_ShouldReturnCreatedAtAction_WhenCreatedSuccessfully()
        {
            var request = _fixture.Create<CustomerAddRequest>();
            var created = _fixture.Build<CustomerResponse>().With(c => c.CustomerId, Guid.NewGuid()).Create();

            _customersAdderServiceMock.Setup(s => s.AddCustomer(request)).ReturnsAsync(created);

            var controller = CreateController();

            var result = await controller.PostCustomer(request);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            createdResult.Value.Should().BeEquivalentTo(created);
        }

        [Fact]
        public async Task PostCustomer_ShouldReturnBadRequest_WhenRequestIsNull()
        {
            var controller = CreateController();

            var result = await controller.PostCustomer(null);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            bad.Value.Should().Be("Customer data is missing.");
        }

        [Fact]
        public async Task PostCustomer_ShouldReturnProblem_WhenAddFails()
        {
            // Arrange
            var request = _fixture.Create<CustomerAddRequest>();
            _customersAdderServiceMock
                .Setup(s => s.AddCustomer(request))
                .ReturnsAsync((CustomerResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.PostCustomer(request);

            // Assert
            var problem = Assert.IsType<ObjectResult>(result);
            var problemDetails = Assert.IsType<ProblemDetails>(problem.Value);
            problemDetails.Detail.Should().Be("An error occurred while saving the customer.");
            problem.StatusCode.Should().Be(500);
        }

        #endregion

        #region DELETE /api/customers/{customerId}

        [Fact]
        public async Task DeleteCustomer_ShouldReturnNoContent_WhenDeletedSuccessfully()
        {
            var id = Guid.NewGuid();
            var existing = _fixture.Build<CustomerResponse>().With(c => c.CustomerId, id).Create();

            _customersGetterServiceMock.Setup(s => s.GetCustomer(id)).ReturnsAsync(existing);
            _customersDeleterServiceMock.Setup(s => s.DeleteCustomer(id)).ReturnsAsync(true);

            var controller = CreateController();

            var result = await controller.DeleteCustomer(id);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteCustomer_ShouldReturnNotFound_WhenCustomerDoesNotExist()
        {
            var id = Guid.NewGuid();
            _customersGetterServiceMock.Setup(s => s.GetCustomer(id)).ReturnsAsync((CustomerResponse?)null);

            var controller = CreateController();

            var result = await controller.DeleteCustomer(id);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteCustomer_ShouldReturnProblem_WhenDeletionFails()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = _fixture.Build<CustomerResponse>()
                                   .With(c => c.CustomerId, id)
                                   .Create();

            _customersGetterServiceMock
                .Setup(s => s.GetCustomer(id))
                .ReturnsAsync(existing);

            _customersDeleterServiceMock
                .Setup(s => s.DeleteCustomer(id))
                .ReturnsAsync(false);

            var controller = CreateController();

            // Act
            var result = await controller.DeleteCustomer(id);

            // Assert
            var problem = Assert.IsType<ObjectResult>(result);
            var problemDetails = Assert.IsType<ProblemDetails>(problem.Value);
            problemDetails.Detail.Should().Be("Failed to delete the customer.");
            problem.StatusCode.Should().Be(500);
        }

        #endregion
        //public class CustomersControllerTest
        //{

        //    private readonly Mock<ICustomersAdderService> _customersAdderServiceMock;
        //    private readonly Mock<ICustomersDeleterService> _customersDeleterServiceMock;
        //    private readonly Mock<ICustomersGetterService> _customersGetterServiceMock;
        //    private readonly Mock<ICustomersUpdaterService> _customersUpdaterServiceMock;
        //    private readonly Mock<ILogger<CustomersController>> _loggerMock;

        //    private readonly Fixture _fixture;

        //    public CustomersControllerTest()
        //    {
        //        _fixture = new Fixture();

        //        // Handle recursion safely
        //        _fixture.Behaviors
        //            .OfType<ThrowingRecursionBehavior>()
        //            .ToList()
        //            .ForEach(b => _fixture.Behaviors.Remove(b));
        //        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        //        _customersAdderServiceMock = new Mock<ICustomersAdderService>();
        //        _customersDeleterServiceMock = new Mock<ICustomersDeleterService>();
        //        _customersGetterServiceMock = new Mock<ICustomersGetterService>();
        //        _customersUpdaterServiceMock = new Mock<ICustomersUpdaterService>();
        //        _loggerMock = new Mock<ILogger<CustomersController>>();
        //    }

        //    private CustomersController CreateController()
        //    {
        //        return new CustomersController(
        //            _customersAdderServiceMock.Object,
        //            _customersDeleterServiceMock.Object,
        //            _customersGetterServiceMock.Object,
        //            _customersUpdaterServiceMock.Object,
        //            _loggerMock.Object
        //        );
        //    }

        //    #region GET /api/customers

        //    [Fact]
        //    public async Task GetCustomers_ShouldReturnOk_WithListOfCustomers()
        //    {
        //        // Arrange
        //        var customers = _fixture.CreateMany<CustomerResponse>(5).ToList();

        //        _customersGetterServiceMock
        //            .Setup(s => s.GetAllCustomers())
        //            .ReturnsAsync(customers);

        //        var controller = CreateController();

        //        // Act
        //        var result = await controller.GetCustomers();

        //        // Assert
        //        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        //        var returnValue = Assert.IsAssignableFrom<IEnumerable<CustomerResponse>>(okResult.Value);
        //        returnValue.Should().BeEquivalentTo(customers);
        //    }

        //    #endregion

        //    #region GET /api/customers/{customerId}

        //    [Fact]
        //    public async Task GetCustomer_ShouldReturnCustomer_WhenFound()
        //    {
        //        // Arrange
        //        var customerId = Guid.NewGuid();
        //        var expectedCustomer = _fixture.Build<CustomerResponse>()
        //                                       .With(c => c.CustomerId, customerId)
        //                                       .Create();

        //        _customersGetterServiceMock
        //            .Setup(s => s.GetCustomer(customerId))
        //            .ReturnsAsync(expectedCustomer);

        //        var controller = CreateController();

        //        // Act
        //        var result = await controller.GetCustomer(customerId);

        //        // Assert
        //        var value = Assert.IsType<ActionResult<CustomerResponse>>(result);
        //        value.Value.Should().BeEquivalentTo(expectedCustomer);
        //    }

        //    #endregion

        //    #region PUT /api/customers/{customerId}

        //    [Fact]
        //    public async Task PutCustomer_ShouldReturnCreatedAtAction_WhenUpdatedSuccessfully()
        //    {
        //        // Arrange
        //        var customerId = Guid.NewGuid();
        //        var updateRequest = _fixture.Build<CustomerUpdateRequest>()
        //                                    .With(c => c.CustomerId, customerId)
        //                                    .Create();

        //        var existingCustomer = _fixture.Build<CustomerResponse>()
        //                                       .With(c => c.CustomerId, customerId)
        //                                       .Create();

        //        var updatedCustomer = _fixture.Build<CustomerResponse>()
        //                                      .With(c => c.CustomerId, customerId)
        //                                      .Create();

        //        _customersGetterServiceMock
        //            .Setup(s => s.GetCustomer(customerId))
        //            .ReturnsAsync(existingCustomer);

        //        _customersUpdaterServiceMock
        //            .Setup(s => s.UpdateCustomer(updateRequest))
        //            .ReturnsAsync(updatedCustomer);

        //        var controller = CreateController();

        //        // Act
        //        var result = await controller.PutCustomer(customerId, updateRequest);

        //        // Assert
        //        var createdAtAction = Assert.IsType<CreatedAtActionResult>(result);
        //        Assert.Equal(nameof(CustomersController.GetCustomer), createdAtAction.ActionName);
        //        var actualResponse = Assert.IsType<CustomerResponse>(createdAtAction.Value);
        //        actualResponse.Should().BeEquivalentTo(updatedCustomer);
        //    }

        //    #endregion

        //    #region POST /api/customers

        //    [Fact]
        //    public async Task PostCustomer_ShouldReturnCreatedAtAction_WhenCreatedSuccessfully()
        //    {
        //        // Arrange
        //        var addRequest = _fixture.Create<CustomerAddRequest>();
        //        var createdCustomer = _fixture.Build<CustomerResponse>()
        //                                      .With(c => c.CustomerId, Guid.NewGuid())
        //                                      .Create();

        //        _customersAdderServiceMock
        //            .Setup(s => s.AddCustomer(addRequest))
        //            .ReturnsAsync(createdCustomer);

        //        var controller = CreateController();

        //        // Act
        //        var result = await controller.PostCustomer(addRequest);

        //        // Assert
        //        var createdAtAction = Assert.IsType<CreatedAtActionResult>(result);
        //        Assert.Equal(nameof(CustomersController.GetCustomer), createdAtAction.ActionName);
        //        var actualResponse = Assert.IsType<CustomerResponse>(createdAtAction.Value);
        //        actualResponse.Should().BeEquivalentTo(createdCustomer);
        //    }

        //    #endregion

        //    #region DELETE /api/customers/{customerId}

        //    [Fact]
        //    public async Task DeleteCustomer_ShouldReturnNoContent_WhenDeletedSuccessfully()
        //    {
        //        // Arrange
        //        var customerId = Guid.NewGuid();
        //        var existingCustomer = _fixture.Build<CustomerResponse>()
        //                                       .With(c => c.CustomerId, customerId)
        //                                       .Create();

        //        _customersGetterServiceMock
        //            .Setup(s => s.GetCustomer(customerId))
        //            .ReturnsAsync(existingCustomer);

        //        _customersDeleterServiceMock
        //            .Setup(s => s.DeleteCustomer(customerId))
        //            .ReturnsAsync(true);

        //        var controller = CreateController();

        //        // Act
        //        var result = await controller.DeleteCustomer(customerId);

        //        // Assert
        //        Assert.IsType<NoContentResult>(result);
        //    }

        //    #endregion
        //}

    }
}
