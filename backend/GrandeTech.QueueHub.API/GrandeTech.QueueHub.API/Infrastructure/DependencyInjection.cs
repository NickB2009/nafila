using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GrandeTech.QueueHub.API.Domain.Common;
using GrandeTech.QueueHub.API.Domain.Users;
using GrandeTech.QueueHub.API.Domain.Organizations;
using GrandeTech.QueueHub.API.Domain.Locations;
using GrandeTech.QueueHub.API.Domain.Queues;
using GrandeTech.QueueHub.API.Domain.Customers;
using GrandeTech.QueueHub.API.Domain.Staff;
using GrandeTech.QueueHub.API.Domain.Services;
using GrandeTech.QueueHub.API.Domain.Subscriptions;
using GrandeTech.QueueHub.API.Domain.AuditLogs;
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
            // Add bogus repositories for in-memory testing
            services.AddScoped(typeof(IRepository<>), typeof(BogusGenericRepository<>));
            services.AddScoped<IUserRepository, BogusUserRepository>();
            services.AddScoped<IOrganizationRepository, BogusOrganizationRepository>();
            services.AddScoped<ILocationRepository, BogusLocationRepository>();
            services.AddScoped<IQueueRepository, BogusQueueRepository>();
            services.AddScoped<ICustomerRepository, BogusCustomerRepository>();
            services.AddScoped<IStaffMemberRepository, BogusStaffMemberRepository>();
            services.AddScoped<IServiceTypeRepository, BogusServiceTypeRepository>();
            services.AddScoped<ISubscriptionPlanRepository, BogusSubscriptionPlanRepository>();
            services.AddScoped<IAuditLogRepository, BogusAuditLogRepository>();
            
            return services;
        }
    }
}
