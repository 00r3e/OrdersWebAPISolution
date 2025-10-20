using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;

namespace RepositoryContracts
{
    public interface ICountryRepository
    {
        Task<ICollection<Country>> GetCountries();

        Task<Country?> GetCountry(Guid countryId);

        Task<Country?> AddCountry(Country country);

        Task<Country?> UpdateCountry(Country country);

        Task<bool> DeleteCountry(Guid countryId);

        Task<bool> Save();
    }
}
