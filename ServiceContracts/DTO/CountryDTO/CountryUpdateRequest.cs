using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;

namespace ServiceContracts.DTO.CountryDTO
{
    public class CountryUpdateRequest
    {
        public Guid CountryId { get; set; }

        public string Name { get; set; }


        /// <summary>
        /// Converts the current object of OrderAddRequest into a new object of Order type
        /// </summary>
        /// <returns></returns>
        public Country ToCountry()
        {
            return new Country
            {
                CountryId = CountryId,
                Name = Name
            };
        }
    }
}
