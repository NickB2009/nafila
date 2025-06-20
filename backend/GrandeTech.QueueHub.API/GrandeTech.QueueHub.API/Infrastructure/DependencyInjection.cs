using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.Subscriptions;
using Grande.Fila.API.Domain.AuditLogs;
using Grande.Fila.API.Infrastructure.Repositories.Bogus;
using Grande.Fila.API.Domain.ServicesOffered;

namespace Grande.Fila.API.Infrastructure
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
            services.AddScoped<IServicesOfferedRepository, BogusServiceTypeRepository>();
            services.AddScoped<ISubscriptionPlanRepository, BogusSubscriptionPlanRepository>();
            services.AddScoped<IAuditLogRepository, BogusAuditLogRepository>();
            
            return services;
        }
    }
}
