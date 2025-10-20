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
using ServiceContracts.DTO.CountryDTO;
using ServiceContracts.ICountriesServices;

namespace Orders.WebAPI.Tests
{
    public class CountriesControllerTest
    {
        private readonly Mock<ICountriesAdderService> _countriesAdderServiceMock;
        private readonly Mock<ICountriesDeleterService> _countriesDeleterServiceMock;
        private readonly Mock<ICountriesGetterService> _countriesGetterServiceMock;
        private readonly Mock<ICountriesUpdaterService> _countriesUpdaterServiceMock;
        private readonly Mock<ILogger<CountriesController>> _loggerMock;
        private readonly Fixture _fixture;

        public CountriesControllerTest()
        {
            _fixture = new Fixture();

            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _countriesAdderServiceMock = new Mock<ICountriesAdderService>();
            _countriesDeleterServiceMock = new Mock<ICountriesDeleterService>();
            _countriesGetterServiceMock = new Mock<ICountriesGetterService>();
            _countriesUpdaterServiceMock = new Mock<ICountriesUpdaterService>();
            _loggerMock = new Mock<ILogger<CountriesController>>();
        }

        private CountriesController CreateController()
        {
            return new CountriesController(
                _countriesAdderServiceMock.Object,
                _countriesDeleterServiceMock.Object,
                _countriesGetterServiceMock.Object,
                _countriesUpdaterServiceMock.Object,
                _loggerMock.Object
            );
        }

        #region GET /api/countries

        [Fact]
        public async Task GetCountries_ShouldReturnOk_WithListOfCountries()
        {
            // Arrange
            var countries = _fixture.CreateMany<CountryResponse>(5).ToList();

            _countriesGetterServiceMock
                .Setup(s => s.GetAllCountries())
                .ReturnsAsync(countries);

            var controller = CreateController();

            // Act
            var result = await controller.GetCountries();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<CountryResponse>>(okResult.Value);
            returnValue.Should().BeEquivalentTo(countries);
        }

        [Fact]
        public async Task GetCountries_ShouldReturnOk_WithEmptyList_WhenNoCountriesExist()
        {
            // Arrange
            _countriesGetterServiceMock
                .Setup(s => s.GetAllCountries())
                .ReturnsAsync(new List<CountryResponse>());

            var controller = CreateController();

            // Act
            var result = await controller.GetCountries();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var countries = Assert.IsAssignableFrom<IEnumerable<CountryResponse>>(okResult.Value);
            countries.Should().BeEmpty();
        }

        #endregion

        #region GET /api/countries/{countryId}

        [Fact]
        public async Task GetCountry_ShouldReturnCountry_WhenFound()
        {
            // Arrange
            var countryId = Guid.NewGuid();
            var expectedCountry = _fixture.Build<CountryResponse>()
                                          .With(c => c.CountryId, countryId)
                                          .Create();

            _countriesGetterServiceMock
                .Setup(s => s.GetCountry(countryId))
                .ReturnsAsync(expectedCountry);

            var controller = CreateController();

            // Act
            var result = await controller.GetCountry(countryId);

            // Assert
            result.Value.Should().BeEquivalentTo(expectedCountry);
        }

        [Fact]
        public async Task GetCountry_ShouldReturnNotFound_WhenCountryDoesNotExist()
        {
            // Arrange
            var countryId = Guid.NewGuid();

            _countriesGetterServiceMock
                .Setup(s => s.GetCountry(countryId))
                .ReturnsAsync((CountryResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.GetCountry(countryId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<CountryResponse>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        #endregion

        #region PUT /api/countries/{countryId}

        [Fact]
        public async Task PutCountry_ShouldReturnCreatedAtAction_WhenUpdatedSuccessfully()
        {
            // Arrange
            var countryId = Guid.NewGuid();
            var updateRequest = _fixture.Build<CountryUpdateRequest>()
                                        .With(c => c.CountryId, countryId)
                                        .Create();

            var existingCountry = _fixture.Build<CountryResponse>()
                                          .With(c => c.CountryId, countryId)
                                          .Create();

            var updatedCountry = _fixture.Build<CountryResponse>()
                                         .With(c => c.CountryId, countryId)
                                         .Create();

            _countriesGetterServiceMock
                .Setup(s => s.GetCountry(countryId))
                .ReturnsAsync(existingCountry);

            _countriesUpdaterServiceMock
                .Setup(s => s.UpdateCountry(updateRequest))
                .ReturnsAsync(updatedCountry);

            var controller = CreateController();

            // Act
            var result = await controller.PutCountry(countryId, updateRequest);

            // Assert
            var createdAt = Assert.IsType<CreatedAtActionResult>(result);
            var actualResponse = Assert.IsType<CountryResponse>(createdAt.Value);
            actualResponse.Should().BeEquivalentTo(updatedCountry);
        }

        [Fact]
        public async Task PutCountry_ShouldReturnBadRequest_WhenIdMismatch()
        {
            // Arrange
            var countryId = Guid.NewGuid();
            var updateRequest = _fixture.Build<CountryUpdateRequest>()
                                        .With(c => c.CountryId, Guid.NewGuid())
                                        .Create();

            var controller = CreateController();

            // Act
            var result = await controller.PutCountry(countryId, updateRequest);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be("Country ID mismatch.");
        }

        [Fact]
        public async Task PutCountry_ShouldReturnNotFound_WhenCountryDoesNotExist()
        {
            // Arrange
            var countryId = Guid.NewGuid();
            var updateRequest = _fixture.Build<CountryUpdateRequest>()
                                        .With(c => c.CountryId, countryId)
                                        .Create();

            _countriesGetterServiceMock
                .Setup(s => s.GetCountry(countryId))
                .ReturnsAsync((CountryResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.PutCountry(countryId, updateRequest);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PutCountry_ShouldReturnProblem_WhenUpdateReturnsNull()
        {
            // Arrange
            var countryId = Guid.NewGuid();
            var request = _fixture.Build<CountryUpdateRequest>()
                                  .With(c => c.CountryId, countryId)
                                  .Create();

            var existing = _fixture.Build<CountryResponse>()
                                   .With(c => c.CountryId, countryId)
                                   .Create();

            _countriesGetterServiceMock.Setup(s => s.GetCountry(countryId)).ReturnsAsync(existing);
            _countriesUpdaterServiceMock.Setup(s => s.UpdateCountry(request)).ReturnsAsync((CountryResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.PutCountry(countryId, request);

            // Assert
            var problem = Assert.IsType<ObjectResult>(result);
            problem.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task PutCountry_ShouldReturnConflict_WhenDbUpdateConcurrencyExceptionOccurs()
        {
            // Arrange
            var countryId = Guid.NewGuid();
            var request = _fixture.Build<CountryUpdateRequest>()
                                  .With(c => c.CountryId, countryId)
                                  .Create();

            var existing = _fixture.Build<CountryResponse>()
                                   .With(c => c.CountryId, countryId)
                                   .Create();

            _countriesGetterServiceMock.Setup(s => s.GetCountry(countryId)).ReturnsAsync(existing);
            _countriesUpdaterServiceMock
                .Setup(s => s.UpdateCountry(request))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            var controller = CreateController();

            // Act
            var result = await controller.PutCountry(countryId, request);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            conflict.Value.Should().Be("A concurrency conflict occurred.");
        }

        #endregion

        #region POST /api/countries

        [Fact]
        public async Task PostCountry_ShouldReturnCreatedAtAction_WhenCreatedSuccessfully()
        {
            // Arrange
            var addRequest = _fixture.Create<CountryAddRequest>();
            var createdCountry = _fixture.Build<CountryResponse>()
                                         .With(c => c.CountryId, Guid.NewGuid())
                                         .Create();

            _countriesAdderServiceMock
                .Setup(s => s.AddCountry(addRequest))
                .ReturnsAsync(createdCountry);

            var controller = CreateController();

            // Act
            var result = await controller.PostCountry(addRequest);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result);
            created.Value.Should().BeEquivalentTo(createdCountry);
        }

        [Fact]
        public async Task PostCountry_ShouldReturnBadRequest_WhenRequestIsNull()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.PostCountry(null);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be("Country data is missing.");
        }

        [Fact]
        public async Task PostCountry_ShouldReturnProblem_WhenAddCountryFails()
        {
            // Arrange
            var addRequest = _fixture.Create<CountryAddRequest>();

            _countriesAdderServiceMock
                .Setup(s => s.AddCountry(addRequest))
                .ReturnsAsync((CountryResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.PostCountry(addRequest);

            // Assert
            var problem = Assert.IsType<ObjectResult>(result);
            problem.StatusCode.Should().Be(500);
        }

        #endregion

        #region DELETE /api/countries/{countryId}

        [Fact]
        public async Task DeleteCountry_ShouldReturnNoContent_WhenDeletedSuccessfully()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = _fixture.Build<CountryResponse>().With(c => c.CountryId, id).Create();

            _countriesGetterServiceMock.Setup(s => s.GetCountry(id)).ReturnsAsync(existing);
            _countriesDeleterServiceMock.Setup(s => s.DeleteCountry(id)).ReturnsAsync(true);

            var controller = CreateController();

            // Act
            var result = await controller.DeleteCountry(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteCountry_ShouldReturnNotFound_WhenCountryDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            _countriesGetterServiceMock.Setup(s => s.GetCountry(id)).ReturnsAsync((CountryResponse?)null);

            var controller = CreateController();

            // Act
            var result = await controller.DeleteCountry(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteCountry_ShouldReturnProblem_WhenDeleteFails()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = _fixture.Build<CountryResponse>().With(c => c.CountryId, id).Create();

            _countriesGetterServiceMock.Setup(s => s.GetCountry(id)).ReturnsAsync(existing);
            _countriesDeleterServiceMock.Setup(s => s.DeleteCountry(id)).ReturnsAsync(false);

            var controller = CreateController();

            // Act
            var result = await controller.DeleteCountry(id);

            // Assert
            var problem = Assert.IsType<ObjectResult>(result);
            problem.StatusCode.Should().Be(500);
        }

        #endregion
    }
    //public class CountriesControllerTest
    //{
    //    private readonly Mock<ICountriesAdderService> _countriesAdderServiceMock;
    //    private readonly Mock<ICountriesDeleterService> _countriesDeleterServiceMock;
    //    private readonly Mock<ICountriesGetterService> _countriesGetterServiceMock;
    //    private readonly Mock<ICountriesUpdaterService> _countriesUpdaterServiceMock;
    //    private readonly Mock<ILogger<CountriesController>> _loggerMock;

    //    private readonly Fixture _fixture;

    //    public CountriesControllerTest()
    //    {
    //        _fixture = new Fixture();

    //        // Handle circular reference issues
    //        _fixture.Behaviors
    //            .OfType<ThrowingRecursionBehavior>()
    //            .ToList()
    //            .ForEach(b => _fixture.Behaviors.Remove(b));
    //        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

    //        _countriesAdderServiceMock = new Mock<ICountriesAdderService>();
    //        _countriesDeleterServiceMock = new Mock<ICountriesDeleterService>();
    //        _countriesGetterServiceMock = new Mock<ICountriesGetterService>();
    //        _countriesUpdaterServiceMock = new Mock<ICountriesUpdaterService>();
    //        _loggerMock = new Mock<ILogger<CountriesController>>();
    //    }

    //    private CountriesController CreateController()
    //    {
    //        return new CountriesController(
    //            _countriesAdderServiceMock.Object,
    //            _countriesDeleterServiceMock.Object,
    //            _countriesGetterServiceMock.Object,
    //            _countriesUpdaterServiceMock.Object,
    //            _loggerMock.Object
    //        );
    //    }

    //    #region GET /api/countries

    //    [Fact]
    //    public async Task GetCountries_ShouldReturnOk_WithListOfCountries()
    //    {
    //        // Arrange
    //        var countries = _fixture.CreateMany<CountryResponse>(5).ToList();

    //        _countriesGetterServiceMock
    //            .Setup(s => s.GetAllCountries())
    //            .ReturnsAsync(countries);

    //        var controller = CreateController();

    //        // Act
    //        var result = await controller.GetCountries();

    //        // Assert
    //        var okResult = Assert.IsType<OkObjectResult>(result.Result);
    //        var returnValue = Assert.IsAssignableFrom<IEnumerable<CountryResponse>>(okResult.Value);
    //        returnValue.Should().BeEquivalentTo(countries);
    //    }

    //    #endregion

    //    #region GET /api/countries/{countryId}

    //    [Fact]
    //    public async Task GetCountry_ShouldReturnCountry_WhenFound()
    //    {
    //        // Arrange
    //        var countryId = Guid.NewGuid();
    //        var expectedCountry = _fixture.Build<CountryResponse>()
    //                                      .With(c => c.CountryId, countryId)
    //                                      .Create();

    //        _countriesGetterServiceMock
    //            .Setup(s => s.GetCountry(countryId))
    //            .ReturnsAsync(expectedCountry);

    //        var controller = CreateController();

    //        // Act
    //        var result = await controller.GetCountry(countryId);

    //        // Assert
    //        var value = Assert.IsType<ActionResult<CountryResponse>>(result);
    //        value.Value.Should().BeEquivalentTo(expectedCountry);
    //    }

    //    #endregion

    //    #region PUT /api/countries/{countryId}

    //    [Fact]
    //    public async Task PutCountry_ShouldReturnCreatedAtAction_WhenUpdatedSuccessfully()
    //    {
    //        // Arrange
    //        var countryId = Guid.NewGuid();
    //        var updateRequest = _fixture.Build<CountryUpdateRequest>()
    //                                    .With(c => c.CountryId, countryId)
    //                                    .Create();

    //        var existingCountry = _fixture.Build<CountryResponse>()
    //                                      .With(c => c.CountryId, countryId)
    //                                      .Create();

    //        var updatedCountry = _fixture.Build<CountryResponse>()
    //                                     .With(c => c.CountryId, countryId)
    //                                     .Create();

    //        _countriesGetterServiceMock
    //            .Setup(s => s.GetCountry(countryId))
    //            .ReturnsAsync(existingCountry);

    //        _countriesUpdaterServiceMock
    //            .Setup(s => s.UpdateCountry(updateRequest))
    //            .ReturnsAsync(updatedCountry);

    //        var controller = CreateController();

    //        // Act
    //        var result = await controller.PutCountry(countryId, updateRequest);

    //        // Assert
    //        var createdAtAction = Assert.IsType<CreatedAtActionResult>(result);
    //        Assert.Equal(nameof(CountriesController.GetCountry), createdAtAction.ActionName);
    //        var actualResponse = Assert.IsType<CountryResponse>(createdAtAction.Value);
    //        actualResponse.Should().BeEquivalentTo(updatedCountry);
    //    }

    //    #endregion

    //    #region POST /api/countries

    //    [Fact]
    //    public async Task PostCountry_ShouldReturnCreatedAtAction_WhenCreatedSuccessfully()
    //    {
    //        // Arrange
    //        var addRequest = _fixture.Create<CountryAddRequest>();
    //        var createdCountry = _fixture.Build<CountryResponse>()
    //                                     .With(c => c.CountryId, Guid.NewGuid())
    //                                     .Create();

    //        _countriesAdderServiceMock
    //            .Setup(s => s.AddCountry(addRequest))
    //            .ReturnsAsync(createdCountry);

    //        var controller = CreateController();

    //        // Act
    //        var result = await controller.PostCountry(addRequest);

    //        // Assert
    //        var createdAtAction = Assert.IsType<CreatedAtActionResult>(result);
    //        Assert.Equal(nameof(CountriesController.GetCountry), createdAtAction.ActionName);
    //        var actualResponse = Assert.IsType<CountryResponse>(createdAtAction.Value);
    //        actualResponse.Should().BeEquivalentTo(createdCountry);
    //    }

    //    #endregion

    //    #region DELETE /api/countries/{countryId}

    //    [Fact]
    //    public async Task DeleteCountry_ShouldReturnNoContent_WhenDeletedSuccessfully()
    //    {
    //        // Arrange
    //        var countryId = Guid.NewGuid();
    //        var existingCountry = _fixture.Build<CountryResponse>()
    //                                      .With(c => c.CountryId, countryId)
    //                                      .Create();

    //        _countriesGetterServiceMock
    //            .Setup(s => s.GetCountry(countryId))
    //            .ReturnsAsync(existingCountry);

    //        _countriesDeleterServiceMock
    //            .Setup(s => s.DeleteCountry(countryId))
    //            .ReturnsAsync(true);

    //        var controller = CreateController();

    //        // Act
    //        var result = await controller.DeleteCountry(countryId);

    //        // Assert
    //        Assert.IsType<NoContentResult>(result);
    //    }

    //    #endregion
}


