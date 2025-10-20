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
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public CustomerRepository(AppDbContext dbContext, IUnitOfWorkManager unitOfWorkManager)
        {
            _dbContext = dbContext;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<Customer?> AddCustomer(Customer customer, List<Guid> countryIds)
        {

            var countries = await _dbContext.Countries
                    .Where(c => countryIds.Contains(c.CountryId))
                    .ToListAsync();


            if (countries.Count != countryIds.Distinct().Count())
            {
                return null; 
            }

            customer.Countries = countries;

            await _dbContext.Customers.AddAsync(customer);

            if (!_unitOfWorkManager.IsUnitOfWorkStarted)
            {
                await Save();

            }

            return customer;
        }

        public async Task<bool> DeleteCustomer(Guid customerId)
        {
            Customer? customer = await _dbContext.Customers.FirstOrDefaultAsync(temp => temp.CustomerId == customerId);
            if ((customer != null))
            {
                _dbContext.Customers.Remove(customer);

                if (!_unitOfWorkManager.IsUnitOfWorkStarted)
                {
                    await Save();
                }
                return true;
            }
            return false;
        }

        public async Task<Customer?> GetCustomer(Guid customerId)
        {
            Customer? customer = await _dbContext.Customers
                .Include(c => c.Orders).Include(c => c.Countries)
                .FirstOrDefaultAsync(temp => temp.CustomerId == customerId);

            return customer;
        }

        public async Task<ICollection<Customer>> GetCustomers()
        {
            return await _dbContext.Customers
                .Include(c => c.Orders).Include(c => c.Countries).ToListAsync();
        }

        public async Task<Customer?> UpdateCustomer(Customer customer, List<Guid> countryIds)
        {
            Customer? matchingCustomer = await _dbContext.Customers.FirstOrDefaultAsync(temp => temp.CustomerId == customer.CustomerId);

            if (matchingCustomer == null)
            {
                return null;
            }

            matchingCustomer.FirstName = customer.FirstName;
            matchingCustomer.LastName = customer.LastName;
            matchingCustomer.Email = customer.Email;
            
            var countries = await _dbContext.Countries
                .Where(c => countryIds.Contains(c.CountryId))
                .ToListAsync();

            if (countries.Count != countryIds.Distinct().Count())
            {
                return null; 
            }

            matchingCustomer.Countries = countries;

            if (!_unitOfWorkManager.IsUnitOfWorkStarted)
            {
                await Save();
            }

            return matchingCustomer;
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
