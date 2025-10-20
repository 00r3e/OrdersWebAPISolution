using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts.DTO.CountryDTO;
using ServiceContracts.ICountriesServices;
using Services.OrderItemsServices;

namespace Services.CountriesServices
{
    public class CountriesGetterService : ICountriesGetterService
    {
        private readonly ICountryRepository _countryRepository;
        private readonly ILogger<CountriesGetterService> _logger;

        public CountriesGetterService(ICountryRepository countryRepository, ILogger<CountriesGetterService> logger)
        {
            _countryRepository = countryRepository;
            _logger = logger;
        }
        public async Task<ICollection<CountryResponse>> GetAllCountries()
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(GetAllCountries), nameof(CountriesGetterService));
            var countries = await _countryRepository.GetCountries();

            return countries.Select(temp => temp.ToCountryResponse()).ToList();
        }

        public async Task<CountryResponse?> GetCountry(Guid countryId)
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(GetCountry), nameof(CountriesGetterService));

            Country? country = await _countryRepository.GetCountry(countryId);

            if (country == null)
            {
                return null;
            }

            return country.ToCountryResponse();
        }
    }
}
