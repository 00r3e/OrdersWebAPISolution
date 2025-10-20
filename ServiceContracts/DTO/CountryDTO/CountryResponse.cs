using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using ServiceContracts.DTO.OrderDTO;

namespace ServiceContracts.DTO.CountryDTO
{
    public class CountryResponse
    {
        public Guid CountryId { get; set; }

        public string Name { get; set; }

    }

    public static class CountryExtensions
    {
        public static CountryResponse ToCountryResponse(this Country country)
        {
            return new CountryResponse()
            {
                CountryId = country.CountryId,
                Name = country.Name

            };
        }
    }
}
