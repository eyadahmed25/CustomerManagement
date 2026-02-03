using AutoMapper;
using CustomerManagement.Repository.Models.Domain;
using CustomerManagement.Services.DTOs;

namespace CustomerManagement.Services.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AddCustomerDTO, Customer>()
                .ForMember(dest => dest.CustomerID, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());

            CreateMap<Customer, CustomerDTO>();
        }
    }
}