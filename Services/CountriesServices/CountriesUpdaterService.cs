using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts.DTO.CountryDTO;
using ServiceContracts.DTO.OrderItemDTO;
using ServiceContracts.ICountriesServices;
using Services.OrderItemsServices;

namespace Services.CountriesServices
{
    public class CountriesUpdaterService : ICountriesUpdaterService
    {
        private readonly ICountryRepository _countryRepository;
        private readonly ILogger<CountriesUpdaterService> _logger;

        public CountriesUpdaterService(ICountryRepository countryRepository, ILogger<CountriesUpdaterService> logger)
        {
            _countryRepository = countryRepository;
            _logger = logger;
        }
        public async Task<CountryResponse?> UpdateCountry(CountryUpdateRequest countryUpdateRequest)
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(UpdateCountry), nameof(CountriesUpdaterService));

            var countryForUpdate = countryUpdateRequest.ToCountry();
            Country? UpdatedCountry = await _countryRepository.UpdateCountry(countryForUpdate);
            if (UpdatedCountry == null) { return null; }
            return UpdatedCountry.ToCountryResponse();
        }
    }
}
