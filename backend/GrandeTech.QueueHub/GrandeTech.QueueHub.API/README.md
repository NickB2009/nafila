# GrandeTech QueueHub API

A comprehensive barbershop queue management system built with .NET 8, Entity Framework Core, and Azure SQL Database.

## 🚀 Production Deployment

### Azure SQL Database Configuration

The application is configured to use Azure SQL Database in production with the following connection string:

```
Server=tcp:grande.database.windows.net,1433;Initial Catalog=barberqueue;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication="Active Directory Default";
```

### Environment Configuration

#### Production (`appsettings.Production.json`)
- **Database**: Azure SQL Database with Active Directory authentication
- **Auto Migration**: Disabled (manual migration required)
- **Seeding**: Disabled (data should be managed separately)
- **Logging**: Warning level only
- **HTTPS**: Enforced

#### Development (`appsettings.Development.json`)
- **Database**: Local SQL Server with test data
- **Auto Migration**: Enabled
- **Seeding**: Enabled with comprehensive test data
- **Logging**: Detailed with sensitive data

### Docker Deployment

```bash
# Build the production image
docker build -t grandetech-queuehub-api .

# Run with production environment
docker run -d \
  --name queuehub-api \
  -p 80:80 \
  -p 443:443 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__AzureSqlConnection="Server=tcp:grande.database.windows.net,1433;Initial Catalog=barberqueue;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";" \
  grandetech-queuehub-api
```

### Health Checks

The application provides health check endpoints for monitoring:

- **`/health`** - Basic health check
- **`/health/ready`** - Readiness check with database connectivity
- **`/Health`** - Custom health controller with detailed status

## 📊 Database Schema

### Core Entities (All Enabled)

| Entity | Records | Description |
|--------|---------|-------------|
| **Users** | 5 | Platform users (Admin, Owner, Staff, Customer, Service) |
| **Organizations** | 2 | Barbershop businesses with branding |
| **Locations** | 3 | Physical shop locations with addresses |
| **Customers** | 5 | Customer profiles with service history |
| **StaffMembers** | 5 | Barbers/staff with skills and availability |
| **SubscriptionPlans** | 4 | Business subscription tiers |
| **ServicesOffered** | 6 | Barbershop services catalog |
| **Queues** | 3 | Real-time queue management |
| **QueueEntries** | 5 | Active customer queue entries |

### Value Objects

- **Slug** - URL-friendly identifiers
- **Email** - Validated email addresses
- **PhoneNumber** - Formatted phone numbers
- **Money** - Currency-aware pricing
- **Address** - Complete address information
- **BrandingConfig** - Custom branding settings
- **TimeSpanRange** - Business hours

## 🔧 Development

### Prerequisites

- .NET 8 SDK
- SQL Server (local or Azure)
- Docker (optional)

### Local Development

```bash
# Clone the repository
git clone <repository-url>
cd backend/GrandeTech.QueueHub

# Restore dependencies
dotnet restore

# Run migrations
cd GrandeTech.QueueHub.API
dotnet ef database update

# Run the application
dotnet run --environment Development
```

### Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📈 Features

### Core Functionality
- ✅ **Queue Management** - Real-time customer queuing
- ✅ **Staff Management** - Barber availability and skills
- ✅ **Service Catalog** - Configurable services with pricing
- ✅ **Business Management** - Multi-location support
- ✅ **Customer Profiles** - Service history and preferences
- ✅ **Subscription Plans** - Business tier management

### Advanced Features
- ✅ **Value Objects** - Type-safe domain modeling
- ✅ **Entity Framework** - Full ORM with migrations
- ✅ **Repository Pattern** - Clean data access
- ✅ **Domain Events** - Event-driven architecture
- ✅ **Health Monitoring** - Production-ready health checks
- ✅ **Docker Support** - Containerized deployment

## 🔒 Security

- **Active Directory Authentication** for Azure SQL Database
- **HTTPS Enforcement** in production
- **Environment-based Configuration** 
- **Sensitive Data Protection** (disabled in production)

## 📝 API Documentation

Swagger UI is available at `/swagger` in all environments.

## 🚀 Deployment Checklist

- [ ] Azure SQL Database created and accessible
- [ ] Active Directory authentication configured
- [ ] Production connection string updated
- [ ] Environment variables set (`ASPNETCORE_ENVIRONMENT=Production`)
- [ ] Database migrations applied
- [ ] Health checks configured
- [ ] HTTPS certificates installed
- [ ] Monitoring and logging configured

## 📞 Support

For production deployment issues, check:
1. Database connectivity (`/health/ready`)
2. Environment configuration
3. Azure SQL Database firewall rules
4. Active Directory permissions 