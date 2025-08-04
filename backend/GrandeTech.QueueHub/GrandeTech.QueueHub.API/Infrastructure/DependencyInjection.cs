using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Data.SqlClient;
using Grande.Fila.API.Domain.Common;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Domain.Organizations;
using Grande.Fila.API.Domain.Locations;
using Grande.Fila.API.Domain.Queues;
using Grande.Fila.API.Domain.Customers;
using Grande.Fila.API.Domain.Staff;
using Grande.Fila.API.Domain.Subscriptions;
using Grande.Fila.API.Domain.AuditLogs;
using Grande.Fila.API.Domain.ServicesOffered;
using Grande.Fila.API.Infrastructure.Data;
using Grande.Fila.API.Infrastructure.Repositories.Bogus;
using Grande.Fila.API.Infrastructure.Repositories.Sql;
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
using Grande.Fila.API.Application.Notifications.Services;
using Grande.Fila.API.Application.Queues.Handlers;

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
            // Add memory cache for cache invalidation handler
            services.AddMemoryCache();
            
            // Database configuration
            AddDataPersistence(services, configuration);
            
            // Add repositories based on configuration
            AddRepositories(services, configuration);
            
            // Add queue services
            services.AddSingleton<IQueueService, InMemoryQueueService>();
            services.AddHostedService<InMemoryQueueService>(provider => 
                (InMemoryQueueService)provider.GetRequiredService<IQueueService>());
            
            // Add queue message handlers
            services.AddScoped<IQueueMessageHandler<SmsNotificationMessage>, SmsNotificationHandler>();
            services.AddScoped<IQueueMessageHandler<AuditLoggingMessage>, AuditLoggingHandler>();
            services.AddScoped<IQueueMessageHandler<CacheInvalidationMessage>, CacheInvalidationHandler>();
            services.AddScoped<IQueueMessageHandler<QueueStateChangeMessage>, QueueStateChangeHandler>();
            
            // Add SMS provider
            services.AddScoped<ISmsProvider, MockSmsProvider>();
            
            // Add application services
            AddApplicationServices(services);
            
            return services;
        }

        private static void AddDataPersistence(IServiceCollection services, IConfiguration configuration)
        {
            var useInMemoryDatabase = configuration.GetValue<bool>("Database:UseInMemoryDatabase", false);
            var useSqlDatabase = configuration.GetValue<bool>("Database:UseSqlDatabase", true);
            var useBogusRepositories = configuration.GetValue<bool>("Database:UseBogusRepositories", false);

            if (useBogusRepositories)
            {
                // When using Bogus repositories, don't register DbContext or health checks
                return;
            }

            if (useSqlDatabase && !useInMemoryDatabase)
            {
                // Validate connection string
                var connectionString = configuration.GetConnectionString("AzureSqlConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "AzureSqlConnection connection string is not configured. " +
                        "Please set ConnectionStrings__AzureSqlConnection environment variable or configure it in Azure App Service.");
                }

                // Note: Using simplified retry logic built into Entity Framework instead of Polly for now

                // Add Entity Framework DbContext for SQL Server with enhanced error handling
                services.AddDbContext<QueueHubDbContext>(options =>
                {
                    var environment = configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT");
                    
                    options.UseSqlServer(connectionString, sqlOptions =>
                    {
                        // Enhanced retry configuration for Azure SQL Database
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(15),
                            errorNumbersToAdd: new[] { 40613, 40501, 40540, 40197, 10928, 10929 });
                        
                        sqlOptions.CommandTimeout(30); // Connection timeout
                    });

                    // Configure additional options based on environment
                    if (environment == "Development")
                    {
                        options.EnableSensitiveDataLogging();
                        options.EnableDetailedErrors();
                    }
                });

                // Add health checks
                services.AddHealthChecks()
                    .AddDbContextCheck<QueueHubDbContext>("database", tags: new[] { "ready", "live" });
            }
            else if (useInMemoryDatabase)
            {
                // Add Entity Framework DbContext for In-Memory database (for testing)
                services.AddDbContext<QueueHubDbContext>(options =>
                {
                    options.UseInMemoryDatabase("QueueHubTestDb");
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });

                // Add health checks for in-memory database
                services.AddHealthChecks()
                    .AddDbContextCheck<QueueHubDbContext>("database", tags: new[] { "ready", "live" });
            }
        }

        private static void AddRepositories(IServiceCollection services, IConfiguration configuration)
        {
            var useInMemoryDatabase = configuration.GetValue<bool>("Database:UseInMemoryDatabase", false);
            var useSqlDatabase = configuration.GetValue<bool>("Database:UseSqlDatabase", true);
            var useBogusRepositories = configuration.GetValue<bool>("Database:UseBogusRepositories", false);

            if (useBogusRepositories || (!useSqlDatabase && !useInMemoryDatabase))
            {
                // Use Bogus repositories for testing/development
                AddBogusRepositories(services);
            }
            else
            {
                // Use SQL repositories for production/testing with actual database
                AddSqlRepositories(services);
            }
        }

        private static void AddBogusRepositories(IServiceCollection services)
        {
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
        }

        private static void AddSqlRepositories(IServiceCollection services)
        {
            // All SQL repository implementations are now complete
            services.AddScoped<IOrganizationRepository, SqlOrganizationRepository>();
            services.AddScoped<IQueueRepository, SqlQueueRepository>();
            services.AddScoped<ILocationRepository, SqlLocationRepository>();
            services.AddScoped<ICustomerRepository, SqlCustomerRepository>();
            services.AddScoped<IUserRepository, SqlUserRepository>();
            services.AddScoped<IStaffMemberRepository, SqlStaffMemberRepository>();
            services.AddScoped<IServicesOfferedRepository, SqlServicesOfferedRepository>();
            services.AddScoped<ISubscriptionPlanRepository, SqlSubscriptionPlanRepository>();
            services.AddScoped<IAuditLogRepository, SqlAuditLogRepository>();
            
            // Add Unit of Work for transaction management
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        private static void AddApplicationServices(IServiceCollection services)
        {
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
            services.AddScoped<UpdateWeeklyHoursService>();
            
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
        }
    }
}
