using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTO.CountryDTO;
using ServiceContracts.DTO.CustomersDTO;
using ServiceContracts.ICountriesServices;
using ServiceContracts.ICustomersServices;

namespace Orders.WebAPI.Controllers
{
    public class CountriesController : CustomControllerBase
    {
        private readonly ICountriesAdderService _countriesAdderService;
        private readonly ICountriesDeleterService _countriesDeleterService;
        private readonly ICountriesGetterService _countriesGetterService;
        private readonly ICountriesUpdaterService _countriesUpdaterService;
        private readonly ILogger<CountriesController> _logger;

        public CountriesController(ICountriesAdderService countriesAdderService, ICountriesDeleterService countriesDeleterService, ICountriesGetterService countriesGetterService,
            ICountriesUpdaterService countriesUpdaterService, ILogger<CountriesController> logger)
        {
            _countriesAdderService = countriesAdderService;
            _countriesDeleterService = countriesDeleterService;
            _countriesGetterService = countriesGetterService;
            _countriesUpdaterService = countriesUpdaterService;
            _logger = logger;
        }

        // GET: api/Countries
        [HttpGet("countries")]
        public async Task<ActionResult<IEnumerable<CountryResponse>>> GetCountries()
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(GetCountries), nameof(CountriesController));
            var countriesRespones = await _countriesGetterService.GetAllCountries();
            return Ok(countriesRespones);
        }

        // GET: api/countries/5
        [HttpGet("countries/{countryId}")]
        public async Task<ActionResult<CountryResponse>> GetCountry(Guid countryId)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(GetCountry), nameof(CountriesController));

            var countryResponse = await _countriesGetterService.GetCountry(countryId);

            if (countryResponse == null)
            {
                return NotFound();
            }

            return countryResponse;
        }

        // PUT: api/countries/5
        [HttpPut("countries/{countryId}")]
        public async Task<IActionResult> PutCountry(Guid countryId, CountryUpdateRequest countryUpdateRequest)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(PutCountry), nameof(CountriesController));

            if (countryId != countryUpdateRequest.CountryId)
            {
                return BadRequest("Country ID mismatch.");
            }

            var existingCountry = await _countriesGetterService.GetCountry(countryId);
            if (existingCountry == null)
            {
                return NotFound();
            }

            try
            {
                CountryResponse? countryResponse = await _countriesUpdaterService.UpdateCountry(countryUpdateRequest);
                if (countryResponse is null)
                {
                    return Problem("Failed to update the country.");
                }

                return CreatedAtAction(nameof(GetCountry), new { countryId = countryResponse.CountryId }, countryResponse);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("A concurrency conflict occurred.");
            }
        }

        // POST: api/Countries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("countries")]
        public async Task<ActionResult> PostCountry(CountryAddRequest countryAddRequest)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(PostCountry), nameof(CountriesController));

            if (countryAddRequest == null)
            {
                return BadRequest("Country data is missing.");
            }

            var countryResponse = await _countriesAdderService.AddCountry(countryAddRequest);

            if (countryResponse == null)
            {
                return Problem("An error occurred while saving the country.");
            }

            return CreatedAtAction(nameof(GetCountry), new { countryId = countryResponse.CountryId }, countryResponse);
        }

        // DELETE: api/Countries/5
        [HttpDelete("countries/{countryId}")]
        public async Task<IActionResult> DeleteCountry (Guid countryId)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(DeleteCountry), nameof(CountriesController));

            var countryResponse = await _countriesGetterService.GetCountry(countryId);
            if (countryResponse == null)
            {
                return NotFound();
            }

            bool isDeleted = await _countriesDeleterService.DeleteCountry(countryId);
            if (!isDeleted)
            {
                return Problem("Failed to delete the country.");
            }

            return NoContent();
        }
    }
}
