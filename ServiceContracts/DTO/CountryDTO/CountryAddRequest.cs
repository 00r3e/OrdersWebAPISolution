using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;

namespace ServiceContracts.DTO.CountryDTO
{
    public class CountryAddRequest
    {
        public string Name { get; set; }

        /// <summary>
        /// Converts the current object of CountryAddRequest into a new object of Country type
        /// </summary>
        /// <returns></returns>
        public Country ToCountry()
        {
            return new Country
            {
                Name = Name
            };
        }
    }
}
