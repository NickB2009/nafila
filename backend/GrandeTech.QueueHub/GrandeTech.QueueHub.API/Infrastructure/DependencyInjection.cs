using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using System.Collections.Generic;
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
using Grande.Fila.API.Domain.Promotions;
using Grande.Fila.API.Infrastructure.Data;
using Grande.Fila.API.Infrastructure.Repositories.Sql;
using Grande.Fila.API.Application.Auth;
using Microsoft.AspNetCore.Identity;
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

            // Security: password hasher for users
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            
            return services;
        }

        private static void AddDataPersistence(IServiceCollection services, IConfiguration configuration)
        {
            var useInMemoryDatabase = configuration.GetValue<bool>("Database:UseInMemoryDatabase", false);
            var useSqlDatabase = configuration.GetValue<bool>("Database:UseSqlDatabase", true);

            if (useSqlDatabase && !useInMemoryDatabase)
            {
                // Validate / obtain connection string
                var connectionString = configuration.GetConnectionString("MySqlConnection");
                var environment = configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") ?? "";

                // Development fallback: use local Docker MySQL container if connection not supplied
                if (string.IsNullOrWhiteSpace(connectionString) && environment == "Development")
                {
                    connectionString = "Server=localhost;Database=QueueHubDb;User=root;Password=DevPassword123!;Port=3306;CharSet=utf8mb4;SslMode=None;ConnectionTimeout=30;";
                }

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "MySqlConnection connection string is not configured. Provide via User Secrets / environment variable or run local MySQL Docker container.");
                }

                services.AddDbContext<QueueHubDbContext>(options =>
                {
                    // Get MySQL configuration from appsettings
                    var mySqlConfig = configuration.GetSection("MySql");
                    var serverVersion = mySqlConfig.GetValue<string>("ServerVersion") ?? "8.0.0";
                    var enableRetryOnFailure = mySqlConfig.GetValue<bool>("EnableRetryOnFailure", true);
                    var maxRetryCount = mySqlConfig.GetValue<int>("MaxRetryCount", 3);
                    var maxRetryDelay = mySqlConfig.GetValue<int>("MaxRetryDelay", 15);
                    var commandTimeout = mySqlConfig.GetValue<int>("CommandTimeout", 30);
                    var maxBatchSize = mySqlConfig.GetValue<int>("MaxBatchSize", 1000);
                    var enableStringComparisonTranslations = mySqlConfig.GetValue<bool>("EnableStringComparisonTranslations", true);
                    var enableSensitiveDataLogging = mySqlConfig.GetValue<bool>("EnableSensitiveDataLogging", false);
                    var enableDetailedErrors = mySqlConfig.GetValue<bool>("EnableDetailedErrors", true);

                    options.UseMySql(connectionString, ServerVersion.Parse(serverVersion), mySqlOptions =>
                    {
                        if (enableRetryOnFailure)
                        {
                            mySqlOptions.EnableRetryOnFailure(
                                maxRetryCount: maxRetryCount,
                                maxRetryDelay: TimeSpan.FromSeconds(maxRetryDelay),
                                errorNumbersToAdd: new List<int> { 1205, 1213, 2006, 2013, 2014, 2015, 2016, 2017, 2018, 2019 });
                        }
                        mySqlOptions.MaxBatchSize(maxBatchSize);
                        
                        if (enableStringComparisonTranslations)
                        {
                            mySqlOptions.EnableStringComparisonTranslations();
                        }
                    });

                    if (enableSensitiveDataLogging || environment == "Development")
                    {
                        options.EnableSensitiveDataLogging();
                    }
                    
                    if (enableDetailedErrors || environment == "Development")
                    {
                        options.EnableDetailedErrors();
                    }
                });

                services.AddHealthChecks()
                    .AddDbContextCheck<QueueHubDbContext>("database", tags: new[] { "ready", "live" });
            }
            else if (useInMemoryDatabase)
            {
                services.AddDbContext<QueueHubDbContext>(options =>
                {
                    options.UseInMemoryDatabase("QueueHubTestDb");
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });

                services.AddHealthChecks()
                    .AddDbContextCheck<QueueHubDbContext>("database", tags: new[] { "ready", "live" });
            }
        }

        private static void AddRepositories(IServiceCollection services, IConfiguration configuration)
        {
            // Always use SQL repositories
            AddSqlRepositories(services);
        }

        private static void AddSqlRepositories(IServiceCollection services)
        {
            // All SQL repository implementations are now complete
            services.AddScoped<IOrganizationRepository, SqlOrganizationRepository>();
            services.AddScoped<IQueueRepository, SqlQueueRepository>();
            services.AddScoped<ILocationRepository, SqlLocationRepository>();
            services.AddScoped<ICouponRepository, SqlCouponRepository>();
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
            services.AddScoped<IQueueAnalyticsService, QueueAnalyticsService>();
            
            // Queue transfer services
            services.AddScoped<IQueueTransferService, QueueTransferService>();
            
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

            // Public API services
            services.AddScoped<Grande.Fila.API.Application.Public.GetPublicSalonsService>();
            services.AddScoped<Grande.Fila.API.Application.Public.GetPublicSalonDetailService>();
            services.AddScoped<Grande.Fila.API.Application.Public.GetPublicQueueStatusService>();
            services.AddScoped<Grande.Fila.API.Application.Public.AnonymousJoinService>();
        }
    }
}
