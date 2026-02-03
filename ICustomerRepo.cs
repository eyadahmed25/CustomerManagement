using System;
using System.Threading.Tasks;
using CustomerManagement.Repository.Models.Domain;
using System.Collections.Generic;

namespace CustomerManagement.Repository.Services
{
    public interface ICustomerRepo
    {
        Task<IEnumerable<Customer>> GetAllAsync(bool onlyActive=true);
        Task<Customer?> GetByIdAsync(Guid id);
        Task<Customer> CreateAsync(Customer customer);
        Task<Customer?> UpdateAsync(Customer customer);
        Task<bool> DeleteAsync(Guid id);
    }
}