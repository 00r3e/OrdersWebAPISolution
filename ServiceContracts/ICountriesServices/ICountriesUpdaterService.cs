using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceContracts.DTO.CountryDTO;
using ServiceContracts.DTO.OrderItemDTO;

namespace ServiceContracts.ICountriesServices
{
    public interface ICountriesUpdaterService
    {
        Task<CountryResponse?> UpdateCountry(CountryUpdateRequest countryUpdateRequest);
    }
}
