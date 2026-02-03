using Microsoft.Extensions.DependencyInjection;
using CustomerManagement.Services.BusinessLogic;
using CustomerManagement.Services.Mappings;

namespace CustomerManagement.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServicesLayer(
            this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
            services.AddScoped<ICustomerService, CustomerService>();
            return services;
        }
    }
}