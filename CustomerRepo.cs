using System;
using Dapper;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using CustomerManagement.Repository.Models.Domain;

namespace CustomerManagement.Repository.Services
{
    public class CustomerRepo : ICustomerRepo
    {
        private readonly string _connectionString;

        public CustomerRepo(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync(bool onlyActive = true)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new { OnlyActive = onlyActive };
            return await connection.QueryAsync<Customer>(
                "sp_GetAllCustomers",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<Customer?> GetByIdAsync(Guid id)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new { CustomerID = id };
            return await connection.QueryFirstOrDefaultAsync<Customer>(
                "sp_GetCustomerById",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<Customer> CreateAsync(Customer customer)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new
            {
                CustomerID = customer.CustomerID,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Nationality = customer.Nationality,
                Email = customer.Email,
                Phone = customer.Phone,
                DateOfBirth = customer.DateOfBirth,
                BloodGroup = customer.BloodGroup,
                Salary = customer.Salary


            };
            var result = await connection.QueryFirstOrDefaultAsync<Customer>(
                "sp_CreateCustomer",
                parameters,
                commandType: CommandType.StoredProcedure);
            return result!;
        }

        public async Task<Customer?> UpdateAsync(Customer customer)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new
            {
                CustomerID = customer.CustomerID,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Nationality = customer.Nationality,
                Email = customer.Email,
                Phone = customer.Phone,
                DateOfBirth = customer.DateOfBirth,
                BloodGroup = customer.BloodGroup,
                Salary = customer.Salary
            };
            var result = await connection.QueryFirstOrDefaultAsync<Customer>(
                "sp_UpdateCustomer",
                parameters,
                commandType: CommandType.StoredProcedure);
            return result?.CustomerID != Guid.Empty ? result : null;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new { CustomerID = id };
            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "sp_DeleteCustomer",
                parameters,
                commandType: CommandType.StoredProcedure);
            return result?.Success == 1;
        }
    }
}