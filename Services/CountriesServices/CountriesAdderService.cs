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
    public class CountriesAdderService : ICountriesAdderService
    {
        private readonly ICountryRepository _countriesRepository;
        private readonly ILogger<CountriesAdderService> _logger;

        public CountriesAdderService(ICountryRepository countriesRepository, ILogger<CountriesAdderService> logger)
        {
            _countriesRepository = countriesRepository;
            _logger = logger;
        }

        public async Task<CountryResponse?> AddCountry(CountryAddRequest countryAddRequest)
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(AddCountry), nameof(CountriesAdderService));

            Country country = countryAddRequest.ToCountry();

            var countryFromAdd = await _countriesRepository.AddCountry(country);

            if (countryFromAdd == null) { return null; }

            return countryFromAdd.ToCountryResponse();
        }
    }
}
