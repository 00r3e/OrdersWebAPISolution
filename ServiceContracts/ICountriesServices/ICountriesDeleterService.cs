using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.ICountriesServices
{
    public interface ICountriesDeleterService
    {
        Task<bool> DeleteCountry(Guid countryId);
    }
}
