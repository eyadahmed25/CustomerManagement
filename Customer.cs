using System;

namespace CustomerManagement.Repository.Models.Domain
{
    public class Customer
    {
        public Guid CustomerID { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Nationality { get; set; }
        public required string Email { get; set; }
        public string? Phone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? BloodGroup { get; set; }
        public decimal? Salary { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}