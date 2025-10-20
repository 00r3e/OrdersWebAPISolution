using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Microsoft.EntityFrameworkCore;
using Orders.WebAPI.ApplicationDbContext;
using RepositoryContracts;
using UnitOfWork;

namespace Repositories
{
    public class CountryRepository : ICountryRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public CountryRepository(AppDbContext dbContext, IUnitOfWorkManager unitOfWorkManager)
        {
            _dbContext = dbContext;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<Country?> AddCountry(Country country)
        {
            await _dbContext.Countries.AddAsync(country);

            if (!_unitOfWorkManager.IsUnitOfWorkStarted)
            {
                await Save();

            }

            return country;
        }

        public async Task<bool> DeleteCountry(Guid countryId)
        {
            Country? country = await _dbContext.Countries.FirstOrDefaultAsync(temp => temp.CountryId == countryId);
            if ((country != null))
            {
                _dbContext.Countries.Remove(country);

                if (!_unitOfWorkManager.IsUnitOfWorkStarted)
                {
                    await Save();
                }
                return true;
            }
            return false;
        }

        public async Task<ICollection<Country>> GetCountries()
        {
            return await _dbContext.Countries.Include("Customers").ToListAsync();
        }

        public async Task<Country?> GetCountry(Guid countryId)
        {
            Country? country = await _dbContext.Countries.FirstOrDefaultAsync(temp => temp.CountryId == countryId);

            return country;
        }

        public async Task<Country?> UpdateCountry(Country country)
        {
            Country? matchingCountry = await _dbContext.Countries.FirstOrDefaultAsync(temp => temp.CountryId == country.CountryId);

            if (matchingCountry == null)
            {
                return null;
            }

            matchingCountry.Name = country.Name;

            if (!_unitOfWorkManager.IsUnitOfWorkStarted)
            {
                await Save();
            }

            return matchingCountry;
        }

        public async Task<bool> Save()
        {
            try
            {
                var result = await _dbContext.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
