
using ServiceContracts.DTO.CountryDTO;


namespace ServiceContracts.ICountriesServices
{
    public interface ICountriesAdderService
    {
        Task<CountryResponse?> AddCountry(CountryAddRequest countryAddRequest);
    }
}
