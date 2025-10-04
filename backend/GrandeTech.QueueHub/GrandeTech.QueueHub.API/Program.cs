using Microsoft.OpenApi.Models;
using Grande.Fila.API.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Grande.Fila.API.Application;
using Grande.Fila.API.Application.Auth;

using Grande.Fila.API.Application.Queues;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Infrastructure.Repositories.Bogus;
using Grande.Fila.API.Application.ServicesOffered;
using Grande.Fila.API.Domain.ServicesOffered;
using Grande.Fila.API.Application.Services;
using Grande.Fila.API.Application.Locations;
using Grande.Fila.API.Application.Services.Cache;
using Grande.Fila.API.Application.Notifications;
using Grande.Fila.API.Application.Notifications.Services;
using Grande.Fila.API.Application.Kiosk;
using Grande.Fila.API.Application.QrCode;
using Grande.Fila.API.Application.Promotions;
using Grande.Fila.API.Domain.Promotions;
using Grande.Fila.API.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using MySqlConnector; // Added for MySqlException specific handling

var builder = WebApplication.CreateBuilder(args);

// Configure for Azure App Service (only if WEBSITES_PORT is set)
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITES_PORT")))
{
    var port = Environment.GetEnvironmentVariable("WEBSITES_PORT");
    builder.WebHost.UseUrls($"http://*:{port}");
}

// Add services to the container.

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    options.EnableAdaptiveSampling = false;
    options.EnablePerformanceCounterCollectionModule = true;
    options.EnableQuickPulseMetricStream = true;
    options.EnableDebugLogger = false;
});

// Add Application Insights logging
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddApplicationInsights();
});

// Add memory cache for response caching
builder.Services.AddMemoryCache();

// Add SignalR for real-time kiosk updates
builder.Services.AddSignalR();

// Register logging service
builder.Services.AddScoped<Grande.Fila.API.Application.Logging.ILoggingService, Grande.Fila.API.Application.Logging.LoggingService>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// WebSocket support is built into ASP.NET Core

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "GrandeTech.QueueHub.API", 
        Version = "v1",
        Description = "REST API for Queue Hub Application"
    });

    // Add JWT Authentication support in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add infrastructure services with error handling
try
{
    builder.Services.AddInfrastructure(builder.Configuration);
}
catch (Exception ex)
{
    // Log the error but don't crash the application
    Console.WriteLine($"Warning: Error during infrastructure registration: {ex.Message}");
    Console.WriteLine("Application will continue but database features may not work properly.");
    
    // Add basic logging even if infrastructure setup fails
    builder.Services.AddLogging();
}

// Add JWT Authentication (only if JWT settings are configured)
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (!string.IsNullOrEmpty(jwtKey) && !string.IsNullOrEmpty(jwtIssuer) && !string.IsNullOrEmpty(jwtAudience))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            // Disable automatic claim mapping to keep original JWT claim types (e.g. "role", "sub")
            options.MapInboundClaims = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });
}
else
{
    // Add authentication without JWT for development/testing
    builder.Services.AddAuthentication();
}

// Add tenant-aware authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequirePlatformAdmin", policy =>
        policy.Requirements.Add(new Grande.Fila.API.Infrastructure.Authorization.TenantRequirement(UserRoles.PlatformAdmin, requireOrganizationContext: false)));
    
    options.AddPolicy("RequireOwner", policy =>
        policy.Requirements.Add(new Grande.Fila.API.Infrastructure.Authorization.TenantRequirement(UserRoles.Owner))); // Business owners with org context
    
    options.AddPolicy("RequireStaff", policy =>
        policy.Requirements.Add(new Grande.Fila.API.Infrastructure.Authorization.TenantRequirement(UserRoles.Staff, requireLocationContext: true)));
    
    options.AddPolicy("RequireCustomer", policy =>
        policy.Requirements.Add(new Grande.Fila.API.Infrastructure.Authorization.TenantRequirement(UserRoles.Customer, requireOrganizationContext: false)));
    
    options.AddPolicy("RequireServiceAccount", policy =>
        policy.Requirements.Add(new Grande.Fila.API.Infrastructure.Authorization.TenantRequirement(UserRoles.ServiceAccount, requireOrganizationContext: false)));
    
    // Legacy policy aliases for backward compatibility
    options.AddPolicy("RequireAdmin", policy =>
        policy.Requirements.Add(new Grande.Fila.API.Infrastructure.Authorization.TenantRequirement(UserRoles.Owner))); // Map to Owner
    
    options.AddPolicy("RequireBarber", policy =>
        policy.Requirements.Add(new Grande.Fila.API.Infrastructure.Authorization.TenantRequirement(UserRoles.Staff, requireLocationContext: true))); // Map to Staff
    
    options.AddPolicy("RequireClient", policy =>
        policy.Requirements.Add(new Grande.Fila.API.Infrastructure.Authorization.TenantRequirement(UserRoles.Customer, requireOrganizationContext: false))); // Map to Customer
});

builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, Grande.Fila.API.Infrastructure.Authorization.TenantAuthorizationHandler>();

// Add tenant context service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Grande.Fila.API.Infrastructure.Authorization.ITenantContextService, Grande.Fila.API.Infrastructure.Authorization.TenantContextService>();

// Register services
builder.Services.AddScoped<AuthService>();
// IUserRepository is already registered in AddInfrastructure

// Register application services
builder.Services.AddScoped<Grande.Fila.API.Application.Locations.CreateLocationService>();
builder.Services.AddScoped<Grande.Fila.API.Application.Locations.ToggleQueueService>();

// Register public API services
builder.Services.AddScoped<Grande.Fila.API.Application.Public.GetPublicSalonsService>();
builder.Services.AddScoped<Grande.Fila.API.Application.Public.GetPublicSalonDetailService>();
builder.Services.AddScoped<Grande.Fila.API.Application.Public.GetPublicQueueStatusService>();
builder.Services.AddScoped<Grande.Fila.API.Application.Public.GetQueueEntryStatusService>();
builder.Services.AddScoped<Grande.Fila.API.Application.Public.LeaveQueueService>();
builder.Services.AddScoped<Grande.Fila.API.Application.Public.UpdateQueueEntryService>();
builder.Services.AddScoped<Grande.Fila.API.Application.Staff.AddBarberService>();
builder.Services.AddScoped<Grande.Fila.API.Application.Staff.EditBarberService>();
builder.Services.AddScoped<Grande.Fila.API.Application.Staff.UpdateStaffStatusService>();
builder.Services.AddScoped<Grande.Fila.API.Application.Staff.StartBreakService>();
builder.Services.AddScoped<Grande.Fila.API.Application.Staff.EndBreakService>();
builder.Services.AddScoped<AddServiceOfferedService>();

// Register organization services
builder.Services.AddScoped<Grande.Fila.API.Application.Organizations.CreateOrganizationService>();
builder.Services.AddScoped<Grande.Fila.API.Application.Organizations.OrganizationService>();

// Register repositories
builder.Services.AddScoped<IServicesOfferedRepository, BogusServiceTypeRepository>();
builder.Services.AddScoped<AddQueueService>();
builder.Services.AddScoped<JoinQueueService>();
builder.Services.AddScoped<BarberAddService>();
builder.Services.AddScoped<CallNextService>();
builder.Services.AddScoped<CheckInService>();
builder.Services.AddScoped<FinishService>();
builder.Services.AddScoped<CancelQueueService>();
builder.Services.AddScoped<SaveHaircutDetailsService>();
builder.Services.AddScoped<EstimatedWaitTimeService>();
builder.Services.AddScoped<ResetAverageService>();

builder.Services.AddSingleton<IAverageWaitTimeCache, Grande.Fila.API.Infrastructure.InMemoryAverageWaitTimeCache>();
builder.Services.AddScoped<UpdateCacheService>();
builder.Services.AddScoped<CalculateWaitService>();
builder.Services.AddScoped<SmsNotificationService>();
builder.Services.AddScoped<KioskDisplayService>();
builder.Services.AddScoped<QrJoinService>();
builder.Services.AddScoped<CouponNotificationService>();
builder.Services.AddScoped<Grande.Fila.API.Application.Public.AnonymousJoinService>();
builder.Services.AddScoped<IQrCodeGenerator, Grande.Fila.API.Infrastructure.MockQrCodeGenerator>();
builder.Services.AddScoped<ISmsProvider, Grande.Fila.API.Infrastructure.MockSmsProvider>();
builder.Services.AddScoped<ICouponRepository, Grande.Fila.API.Infrastructure.Repositories.Bogus.BogusCouponRepository>();

// Register kiosk services
builder.Services.AddScoped<Grande.Fila.API.Infrastructure.Services.IKioskNotificationService, Grande.Fila.API.Infrastructure.Services.KioskNotificationService>();

// Register database performance services
builder.Services.AddScoped<Grande.Fila.API.Infrastructure.Services.IQueryOptimizationService, Grande.Fila.API.Infrastructure.Services.QueryOptimizationService>();
builder.Services.AddScoped<Grande.Fila.API.Infrastructure.Services.IConnectionPoolingService, Grande.Fila.API.Infrastructure.Services.ConnectionPoolingService>();
builder.Services.AddScoped<Grande.Fila.API.Infrastructure.Services.IQueryCacheService, Grande.Fila.API.Infrastructure.Services.QueryCacheService>();

// Add enhanced logging and monitoring services
builder.Services.AddScoped<Grande.Fila.API.Infrastructure.Logging.EnhancedLoggingService>();

// Add security and privacy services
builder.Services.Configure<Grande.Fila.API.Infrastructure.Middleware.RateLimitingOptions>(builder.Configuration.GetSection("RateLimiting"));
builder.Services.Configure<Grande.Fila.API.Infrastructure.Services.DataAnonymizationOptions>(builder.Configuration.GetSection("DataAnonymization"));
builder.Services.Configure<Grande.Fila.API.Infrastructure.Services.SecurityAuditOptions>(builder.Configuration.GetSection("SecurityAudit"));

builder.Services.AddScoped<Grande.Fila.API.Infrastructure.Services.IDataAnonymizationService, Grande.Fila.API.Infrastructure.Services.DataAnonymizationService>();
builder.Services.AddScoped<Grande.Fila.API.Infrastructure.Services.ISecurityAuditService, Grande.Fila.API.Infrastructure.Services.SecurityAuditService>();

// Add performance monitoring services
builder.Services.Configure<Grande.Fila.API.Infrastructure.Services.PerformanceMonitoringOptions>(builder.Configuration.GetSection("PerformanceMonitoring"));
builder.Services.AddScoped<Grande.Fila.API.Infrastructure.Services.IPerformanceMonitoringService, Grande.Fila.API.Infrastructure.Services.PerformanceMonitoringService>();

// Add database seeding service
builder.Services.AddScoped<Grande.Fila.API.Infrastructure.Data.DatabaseSeeder>();

builder.Services.AddSingleton<IAuthorizationHandler, HasPermissionHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HasPermission", policy =>
        policy.Requirements.Add(new HasPermissionRequirement("")));
});

// Health checks are already registered in AddInfrastructure

var app = builder.Build();

// Startup validation and logging (simplified to prevent startup delays)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        
        logger?.LogInformation("Application starting up...");
        logger?.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
        logger?.LogInformation("Listening on port: {Port}", Environment.GetEnvironmentVariable("WEBSITES_PORT") ?? "80");
        
        // Quick connection string validation without detailed parsing
        var connectionString = config.GetConnectionString("MySqlConnection");
        logger?.LogInformation("Database connection string configured: {HasConnectionString}", !string.IsNullOrEmpty(connectionString));
        
        logger?.LogInformation("Database settings - UseSqlDatabase: {UseSql}, UseInMemoryDatabase: {UseInMemory}",
            config.GetValue<bool>("Database:UseSqlDatabase", true),
            config.GetValue<bool>("Database:UseInMemoryDatabase", false));
    }
}
catch (Exception ex)
{
    // Don't let startup validation fail the entire application
    Console.WriteLine($"Startup validation warning: {ex.Message}");
}

// Configure database and seeding based on environment
var useSqlDatabase = builder.Configuration.GetValue<bool>("Database:UseSqlDatabase", true);
var useInMemoryDatabase = builder.Configuration.GetValue<bool>("Database:UseInMemoryDatabase", false);
var useBogusRepositories = builder.Configuration.GetValue<bool>("Database:UseBogusRepositories", false);
var autoMigrate = builder.Configuration.GetValue<bool>("Database:AutoMigrate", false);
var seedData = builder.Configuration.GetValue<bool>("Database:SeedData", false);
var forceReseed = builder.Configuration.GetValue<bool>("Database:ForceReseed", false);

// Environment-specific configuration
var environment = app.Environment.EnvironmentName;
var isDevelopment = environment == "Development";
var isProduction = environment == "Production";

if (isProduction)
{
    // Production-specific settings
    app.UseHsts();
    
    // Ensure HTTPS in production
    app.Use((context, next) =>
    {
        context.Request.Scheme = "https";
        return next();
    });
    
    // Application Insights telemetry is automatically enabled via AddApplicationInsightsTelemetry
}

// Database migration and seeding (when explicitly enabled, regardless of environment)
if ((autoMigrate || seedData) && useSqlDatabase)
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
            var context = scope.ServiceProvider.GetRequiredService<Grande.Fila.API.Infrastructure.Data.QueueHubDbContext>();

            if (autoMigrate)
            {
                logger?.LogInformation("Database migration temporarily disabled for testing...");
                // await context.Database.MigrateAsync();
                // logger?.LogInformation("Database migration completed successfully");
            }

            if (seedData)
            {
                logger?.LogInformation("Attempting database seeding...");
                var seeder = scope.ServiceProvider.GetRequiredService<Grande.Fila.API.Infrastructure.Data.DatabaseSeeder>();
                await seeder.SeedAsync();
                logger?.LogInformation("Database seeding completed successfully");
            }
        }
    }
    catch (MySqlException ex) when (ex.Number == 1045) // MySQL access denied
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
        logger?.LogError(ex, "MySQL access denied (Error 1045). Check username/password in connection string. Message: {Message}", ex.Message);
        logger?.LogWarning("Skipping migration/seeding due to authentication failure. Application will continue without database updates.");
    }
    catch (MySqlException ex)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
        logger?.LogError(ex, "MySQL error during migration/seeding. Number={Number} Message: {Message}", ex.Number, ex.Message);
        logger?.LogWarning("Application will continue without applying migrations/seeding.");
        Console.WriteLine($"Warning: MySQL migration/seeding failed (#{ex.Number}): {ex.Message}");
    }
    catch (Exception ex)
    {
        using (var scope = app.Services.CreateScope())
        {
            var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
            logger?.LogError(ex, "Database migration/seeding failed. Application will continue without seeded data. Error: {ErrorMessage}", ex.Message);
        }

        // Don't crash the application - continue startup
        Console.WriteLine($"Warning: Database migration/seeding failed: {ex.Message}");
        Console.WriteLine("Application will continue but may not have initial data.");
    }
}

// Configure the HTTP request pipeline.
// Swagger is available in all environments
app.UseSwagger();
app.UseSwaggerUI(options => 
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "GrandeTech.QueueHub.API v1");
    options.RoutePrefix = "swagger";
});

// Add correlation ID middleware (very early in pipeline)
app.UseMiddleware<Grande.Fila.API.Infrastructure.Middleware.CorrelationIdMiddleware>();

// Add security headers middleware (early in pipeline)
app.UseMiddleware<Grande.Fila.API.Infrastructure.Middleware.SecurityHeadersMiddleware>();

// Add request size limiting middleware
app.UseMiddleware<Grande.Fila.API.Infrastructure.Middleware.RequestSizeLimitingMiddleware>();

// Add global exception handler middleware
app.UseMiddleware<Grande.Fila.API.Infrastructure.Middleware.GlobalExceptionHandler>();

// Add rate limiting middleware
app.UseMiddleware<Grande.Fila.API.Infrastructure.Middleware.RateLimitingMiddleware>();

// Add response caching middleware for GET endpoints
app.UseMiddleware<Grande.Fila.API.Infrastructure.Middleware.ResponseCachingMiddleware>();

// Add CORS middleware
app.UseCors();

// Only redirect to HTTPS in production (or if explicitly enabled via config)
if (isProduction || builder.Configuration.GetValue<bool>("EnableHttpsRedirect", false))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR hub for kiosk displays
app.MapHub<Grande.Fila.API.Infrastructure.Hubs.KioskDisplayHub>("/kioskHub");

// WebSocket endpoint for queue updates (legacy)
app.Map("/queueHub", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await HandleWebSocketConnection(webSocket);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

// WebSocket connection handler
static async Task HandleWebSocketConnection(System.Net.WebSockets.WebSocket webSocket)
{
    var buffer = new byte[1024 * 4];
    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    
    while (!result.CloseStatus.HasValue)
    {
        // Echo back the message for now
        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer, 0, result.Count),
            result.MessageType,
            result.EndOfMessage,
            CancellationToken.None);
        
        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    }
    
    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
}

// Add a simple root endpoint for basic connectivity testing
app.MapGet("/", () => new { 
    status = "online", 
    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
    port = Environment.GetEnvironmentVariable("WEBSITES_PORT") ?? "80",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
});

// Add a simple ping endpoint
app.MapGet("/ping", () => "pong");

// Health check endpoints with detailed responses
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString(),
                exception = x.Value.Exception?.Message,
                duration = x.Value.Duration.TotalMilliseconds
            })
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Where(x => x.Value.Tags.Contains("ready")).Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString(),
                exception = x.Value.Exception?.Message,
                duration = x.Value.Duration.TotalMilliseconds
            })
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Where(x => x.Value.Tags.Contains("live")).Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString(),
                exception = x.Value.Exception?.Message,
                duration = x.Value.Duration.TotalMilliseconds
            })
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

// Add diagnostic endpoints for troubleshooting
app.MapGet("/diagnostics/config", (IConfiguration config) =>
{
    var connectionString = config.GetConnectionString("MySqlConnection");
    var serverName = "";
    var databaseName = "";
    
    if (!string.IsNullOrEmpty(connectionString))
    {
        try
        {
            var connectionStringBuilder = new MySqlConnectionStringBuilder(connectionString);
            serverName = connectionStringBuilder.Server;
            databaseName = connectionStringBuilder.Database;
        }
        catch (Exception)
        {
            // Ignore parsing errors for diagnostics
        }
    }
    
    return new
    {
        hasConnectionString = !string.IsNullOrEmpty(connectionString),
        connectionStringLength = connectionString?.Length ?? 0,
        serverName = serverName,
        databaseName = databaseName,
        environment = config.GetValue<string>("ASPNETCORE_ENVIRONMENT"),
        useSqlDatabase = config.GetValue<bool>("Database:UseSqlDatabase", true),
        useInMemoryDatabase = config.GetValue<bool>("Database:UseInMemoryDatabase", false),
        useBogusRepositories = config.GetValue<bool>("Database:UseBogusRepositories", false),
        applicationInsightsConfigured = !string.IsNullOrEmpty(config["ApplicationInsights:ConnectionString"])
    };
}).WithTags("Diagnostics");

app.MapGet("/diagnostics/db-test", async (HttpContext httpContext) =>
{
    try
    {
        var serviceProvider = httpContext.RequestServices;
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Grande.Fila.API.Infrastructure.Data.QueueHubDbContext>();
        var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var canConnect = await context.Database.CanConnectAsync();
        stopwatch.Stop();
        
        logger?.LogInformation("Database connection test completed. CanConnect: {CanConnect}, Duration: {Duration}ms", 
            canConnect, stopwatch.ElapsedMilliseconds);
        
        var result = new
        {
            canConnect = canConnect,
            connectionTimeMs = stopwatch.ElapsedMilliseconds,
            timestamp = DateTime.UtcNow
        };
        
        await httpContext.Response.WriteAsJsonAsync(result);
    }
    catch (Exception ex)
    {
        var serviceProvider = httpContext.RequestServices;
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
        logger?.LogError(ex, "Database connection test failed");
        
        var result = new
        {
            canConnect = false,
            error = ex.Message,
            errorType = ex.GetType().Name,
            timestamp = DateTime.UtcNow
        };
        
        httpContext.Response.StatusCode = 500;
        await httpContext.Response.WriteAsJsonAsync(result);
    }
}).WithTags("Diagnostics");

app.Run();

// Make the Program class public for testing
public partial class Program { }
