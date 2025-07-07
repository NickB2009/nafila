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
using Grande.Fila.API.Application.Auth;
using Grande.Fila.API.Application.Organizations;
using Grande.Fila.API.Application.Locations;
using Grande.Fila.API.Application.SubscriptionPlans;
using Grande.Fila.API.Application.Queues;
using Grande.Fila.API.Application.Staff;
using Grande.Fila.API.Application.Services;
using Grande.Fila.API.Application.ServicesOffered;
using Grande.Fila.API.Application.Notifications;
using Grande.Fila.API.Application.Promotions;
using Grande.Fila.API.Application.QrCode;
using Grande.Fila.API.Application.Kiosk;
using Grande.Fila.API.Application.Analytics;

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
            
            // Add application services
            // Auth services
            services.AddScoped<AuthService>();
            
            // Organization services
            services.AddScoped<CreateOrganizationService>();
            services.AddScoped<OrganizationService>();
            services.AddScoped<TrackLiveActivityService>();
            
            // Subscription plan services
            services.AddScoped<CreateSubscriptionPlanService>();
            services.AddScoped<SubscriptionPlanService>();
            
            // Location services
            services.AddScoped<CreateLocationService>();
            services.AddScoped<ResetAverageService>();
            services.AddScoped<ToggleQueueService>();
            
            // Queue services
            services.AddScoped<AddQueueService>();
            services.AddScoped<JoinQueueService>();
            services.AddScoped<BarberAddService>();
            services.AddScoped<CallNextService>();
            services.AddScoped<CheckInService>();
            services.AddScoped<FinishService>();
            services.AddScoped<CancelQueueService>();
            services.AddScoped<SaveHaircutDetailsService>();
            
            // Staff services
            services.AddScoped<AddBarberService>();
            services.AddScoped<EditBarberService>();
            services.AddScoped<StartBreakService>();
            services.AddScoped<EndBreakService>();
            services.AddScoped<UpdateStaffStatusService>();
            
            // General services
            services.AddScoped<CalculateWaitService>();
            services.AddScoped<EstimatedWaitTimeService>();
            services.AddScoped<UpdateCacheService>();
            
            // Analytics services
            services.AddScoped<AnalyticsService>();
            
            // Services offered
            services.AddScoped<AddServiceOfferedService>();
            
            // Notification services
            services.AddScoped<SmsNotificationService>();
            
            // Promotion services
            services.AddScoped<CouponNotificationService>();
            
            // QR Code services
            services.AddScoped<QrJoinService>();
            
            // Kiosk services
            services.AddScoped<KioskDisplayService>();
            
            return services;
        }
    }
}
