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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

// Add infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Add JWT Authentication
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
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found")))
        };
    });

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
builder.Services.AddScoped<Grande.Fila.API.Domain.Queues.IQueueRepository, BogusQueueRepository>();

builder.Services.AddSingleton<IAverageWaitTimeCache, Grande.Fila.API.Infrastructure.InMemoryAverageWaitTimeCache>();
builder.Services.AddScoped<UpdateCacheService>();
builder.Services.AddScoped<CalculateWaitService>();
builder.Services.AddScoped<SmsNotificationService>();
builder.Services.AddScoped<KioskDisplayService>();
builder.Services.AddScoped<QrJoinService>();
builder.Services.AddScoped<CouponNotificationService>();
builder.Services.AddScoped<IQrCodeGenerator, Grande.Fila.API.Infrastructure.MockQrCodeGenerator>();
builder.Services.AddScoped<ISmsProvider, Grande.Fila.API.Infrastructure.MockSmsProvider>();
builder.Services.AddScoped<ICouponRepository, Grande.Fila.API.Infrastructure.Repositories.Bogus.BogusCouponRepository>();

// Add database seeding service
builder.Services.AddScoped<Grande.Fila.API.Infrastructure.Data.DatabaseSeeder>();

builder.Services.AddSingleton<IAuthorizationHandler, HasPermissionHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HasPermission", policy =>
        policy.Requirements.Add(new HasPermissionRequirement("")));
});

var app = builder.Build();

// Configure database and seeding
var autoMigrate = builder.Configuration.GetValue<bool>("Database:AutoMigrate", false);
var seedData = builder.Configuration.GetValue<bool>("Database:SeedData", false);
var useSqlDatabase = builder.Configuration.GetValue<bool>("Database:UseSqlDatabase", true);

if ((autoMigrate || seedData) && useSqlDatabase)
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<Grande.Fila.API.Infrastructure.Data.QueueHubDbContext>();
        
        if (autoMigrate)
        {
            await context.Database.MigrateAsync();
        }
        
        if (seedData)
        {
            var seeder = scope.ServiceProvider.GetRequiredService<Grande.Fila.API.Infrastructure.Data.DatabaseSeeder>();
            await seeder.SeedAsync();
        }
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

// Add CORS middleware
app.UseCors();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Make the Program class public for testing
public partial class Program { }
