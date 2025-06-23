using Microsoft.OpenApi.Models;
using Grande.Fila.API.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Grande.Fila.API.Application.Auth;
using Grande.Fila.API.Application.Queues;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Infrastructure.Repositories.Bogus;
using Grande.Fila.API.Application.ServicesOffered;
using Grande.Fila.API.Domain.ServicesOffered;
using Grande.Fila.API.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
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
        policy.Requirements.Add(new Grande.Fila.API.Infrastructure.Authorization.TenantRequirement("PlatformAdmin", requireOrganizationContext: false)));
    
    options.AddPolicy("RequireAdmin", policy =>
        policy.Requirements.Add(new Grande.Fila.API.Infrastructure.Authorization.TenantRequirement("Admin")));
    
    options.AddPolicy("RequireOwner", policy =>
        policy.Requirements.Add(new Grande.Fila.API.Infrastructure.Authorization.TenantRequirement("Owner")));
    
    options.AddPolicy("RequireBarber", policy =>
        policy.Requirements.Add(new Grande.Fila.API.Infrastructure.Authorization.TenantRequirement("Barber", requireLocationContext: true)));
    
    options.AddPolicy("RequireClient", policy =>
        policy.Requirements.Add(new Grande.Fila.API.Infrastructure.Authorization.TenantRequirement("Client", requireOrganizationContext: false)));
    
    options.AddPolicy("RequireServiceAccount", policy =>
        policy.Requirements.Add(new Grande.Fila.API.Infrastructure.Authorization.TenantRequirement("ServiceAccount", requireOrganizationContext: false)));
});

builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, Grande.Fila.API.Infrastructure.Authorization.TenantAuthorizationHandler>();

// Add tenant context service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Grande.Fila.API.Infrastructure.Authorization.ITenantContextService, Grande.Fila.API.Infrastructure.Authorization.TenantContextService>();

// Register services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IUserRepository, BogusUserRepository>();

// Register application services
builder.Services.AddScoped<Grande.Fila.API.Application.Locations.CreateLocationService>();
builder.Services.AddScoped<Grande.Fila.API.Application.Staff.AddBarberService>();
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
builder.Services.AddScoped<CallNextService>();
builder.Services.AddScoped<CheckInService>();
builder.Services.AddScoped<FinishService>();
builder.Services.AddScoped<CancelQueueService>();
builder.Services.AddScoped<EstimatedWaitTimeService>();
builder.Services.AddScoped<Grande.Fila.API.Domain.Queues.IQueueRepository, BogusQueueRepository>();

var app = builder.Build();

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
