using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CustomerManagement.Repository.Models.Domain;
using CustomerManagement.Repository.Services;
using CustomerManagement.Services.DTOs;
using AutoMapper;
using CustomerManagement.Services.Mappings;
using System.Text.RegularExpressions;
using System.Linq;
using CustomerManagement.ExternalServices.Clients;

namespace CustomerManagement.Services.BusinessLogic
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepo _customerRepo;
        private readonly IMapper _mapper;
        private readonly ITwilioClient _twilioClient;
        private readonly ISendGridClient _sendGridClient;

        public CustomerService(
            ICustomerRepo customerRepo,
            IMapper mapper,
            ITwilioClient twilioClient,
            ISendGridClient sendGridClient)
        {
            _customerRepo = customerRepo;
            _mapper = mapper;
            _twilioClient = twilioClient;
            _sendGridClient = sendGridClient;
        }

        public async Task<IEnumerable<CustomerDTO>> GetAllAsync(bool onlyActive = true)
        {
            var customers = await _customerRepo.GetAllAsync(onlyActive);
            return _mapper.Map<IEnumerable<CustomerDTO>>(customers);
        }

        public async Task<CustomerDTO?> GetByIdAsync(Guid id)
        {
            var customer = await _customerRepo.GetByIdAsync(id);
            if (customer == null)
            {
                return null;
            }
            return _mapper.Map<CustomerDTO>(customer);
        }

        public async Task<CustomerDTO> CreateAsync(AddCustomerDTO dto)
        {
            if (!IsValidEmail(dto.Email))
            {
                throw new ArgumentException("Invalid email format.");
            }
            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                var phoneValidation = await _twilioClient.ValidatePhoneAsync(dto.Phone);
                if (!phoneValidation.IsValid)
                {
                    throw new ArgumentException(
                        $"Invalid phone number: {phoneValidation.ErrorMessage ?? "Phone number validation failed"}");

                }
            }
            var customers = await _customerRepo.GetAllAsync(false);
            if (customers.Any(c => c.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Email already exists.");
            }
            if (dto.DateOfBirth.HasValue)
            {
                int age = CalculateAge(dto.DateOfBirth.Value);
                if (age < 18)
                {
                    throw new InvalidOperationException("Customer must be at least 18 years old.");
                }
            }
            if (string.IsNullOrWhiteSpace(dto.Nationality))
            {
                throw new ArgumentException("Nationality is requied.");
            }
            var customer = _mapper.Map<Customer>(dto);
            customer.CustomerID = Guid.NewGuid();
            customer.IsActive = true;
            customer.CreatedDate = DateTime.Now;
            var createdCustomer = await _customerRepo.CreateAsync(customer);
            _ = Task.Run(async () =>
            {
                try
                {
                    await _sendGridClient.SendWelcomeEmailAsync(createdCustomer.Email, createdCustomer.FirstName);
                }
                catch (Exception ex)
                {
                }
            });
            return _mapper.Map<CustomerDTO>(createdCustomer);
        }
        public async Task<CustomerDTO?> UpdateAsync(Guid id, AddCustomerDTO dto)
        {
            var existingCustomer = await _customerRepo.GetByIdAsync(id);
            if (existingCustomer == null)
            {
                return null;
            }
            if (!IsValidEmail(dto.Email))
            {
                throw new ArgumentException("Invalid email format.");
            }
            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                var phoneValidation = await _twilioClient.ValidatePhoneAsync(dto.Phone);
                if (!phoneValidation.IsValid)
                {
                    throw new ArgumentException(
                        $"Invalid phone number: {phoneValidation.ErrorMessage ?? "Phone number validation failed"}");

                }
            }
            var customers = await _customerRepo.GetAllAsync(false);
            var emailConflict = customers.FirstOrDefault(c => c.Email.Equals(dto.Email,
                StringComparison.OrdinalIgnoreCase) && c.CustomerID != id);
            if (emailConflict != null)
            {
                throw new InvalidOperationException("Email already exists.");
            }
            if (dto.DateOfBirth.HasValue)
            {
                int age = CalculateAge(dto.DateOfBirth.Value);
                if (age < 18)
                {
                    throw new InvalidOperationException("Customer must be at least 18 years old.");
                }
            }
            if (string.IsNullOrWhiteSpace(dto.Nationality))
            {
                throw new ArgumentException("Nationality is requied.");
            }
            _mapper.Map(dto,existingCustomer);
            existingCustomer.CustomerID = id;
            var updatedCustomer = await _customerRepo.UpdateAsync(existingCustomer);
            return updatedCustomer != null ? _mapper.Map<CustomerDTO>(updatedCustomer) : null;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _customerRepo.DeleteAsync(id);
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            try
            {
                var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}