using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;

namespace RepositoryContracts
{
    public interface ICustomerRepository
    {
        Task<ICollection<Customer>> GetCustomers();

        Task<Customer?> GetCustomer(Guid customerId);

        Task<Customer?> AddCustomer(Customer customer, List<Guid> countryIds);

        Task<Customer?> UpdateCustomer(Customer customer, List<Guid> countryIds);

        Task<bool> DeleteCustomer(Guid customerId);

        Task<bool> Save();
    }
}
