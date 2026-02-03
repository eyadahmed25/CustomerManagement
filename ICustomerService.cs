using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerManagement.Services.DTOs;

namespace CustomerManagement.Services.BusinessLogic
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDTO>> GetAllAsync(bool onlyActive = true);
        Task<CustomerDTO?> GetByIdAsync(Guid id);
        Task<CustomerDTO> CreateAsync(AddCustomerDTO dto);
        Task<CustomerDTO?> UpdateAsync(Guid id, AddCustomerDTO dto);
        Task<bool> DeleteAsync(Guid id);
    }
}