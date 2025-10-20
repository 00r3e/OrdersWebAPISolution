using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts.ICountriesServices;
using Services.OrderItemsServices;

namespace Services.CountriesServices
{
    public class CountriesDeleterService : ICountriesDeleterService
    {
        private readonly ICountryRepository _countryRepository;
        private readonly ILogger<CountriesDeleterService> _logger;

        public CountriesDeleterService(ICountryRepository countryRepository, ILogger<CountriesDeleterService> logger)
        {
            _countryRepository = countryRepository;
            _logger = logger;
        }


        public async Task<bool> DeleteCountry(Guid countryId)
        {
            _logger.LogInformation("{MetodName} action method of {ServiceName}", nameof(DeleteCountry), nameof(CountriesDeleterService));

            bool isDeleted = await _countryRepository.DeleteCountry(countryId);

            return isDeleted;
        }
    }
}
