using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GrandeTech.QueueHub.API.Domain.Common;
using GrandeTech.QueueHub.API.Domain.Users;
using GrandeTech.QueueHub.API.Domain.Organizations;
using GrandeTech.QueueHub.API.Domain.ServiceProviders;
using GrandeTech.QueueHub.API.Domain.Queues;
using GrandeTech.QueueHub.API.Domain.Customers;
using GrandeTech.QueueHub.API.Domain.Staff;
using GrandeTech.QueueHub.API.Domain.Services;
using GrandeTech.QueueHub.API.Domain.Subscriptions;
using GrandeTech.QueueHub.API.Infrastructure.Persistence;
using GrandeTech.QueueHub.API.Infrastructure.Repositories;
using GrandeTech.QueueHub.API.Infrastructure.Repositories.Bogus;

namespace GrandeTech.QueueHub.API.Infrastructure
{
    /// <summary>
    /// Extension methods for service registration
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds infrastructure dependencies
        /// </summary>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext (commented out since we're using bogus repositories)
            // services.AddDbContext<ApplicationDbContext>(options =>
            //     options.UseSqlServer(
            //         configuration.GetConnectionString("DefaultConnection"),
            //         b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
                
            // Add UnitOfWork (commented out since we're using bogus repositories)
            // services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // Add bogus repositories
            services.AddScoped(typeof(IRepository<>), typeof(BogusGenericRepository<>));
            services.AddScoped<IUserRepository, BogusUserRepository>();
            services.AddScoped<IOrganizationRepository, BogusOrganizationRepository>();
            services.AddScoped<IServiceProviderRepository, BogusServiceProviderRepository>();
            services.AddScoped<IQueueRepository, BogusQueueRepository>();
            services.AddScoped<ICustomerRepository, BogusCustomerRepository>();
            services.AddScoped<IStaffMemberRepository, BogusStaffMemberRepository>();
            services.AddScoped<IServiceTypeRepository, BogusServiceTypeRepository>();
            services.AddScoped<ISubscriptionPlanRepository, BogusSubscriptionPlanRepository>();
            
            return services;
        }
    }
}
