# EuTôNaFila QueueHub API

A modern, multi-tenant queue management system built with .NET 8, designed for barbershops, clinics, and other service-based businesses.

## 🏗️ Architecture Overview

### Monolithic API Architecture

The QueueHub/EuTôNaFila platform uses a single, comprehensive API that serves all client types through specialized controllers:

- **Core API**: Centralized backend with business logic, data models, and multi-tenant support
- **Specialized Controllers**: Different endpoints optimized for various client types:
  - **PublicController**: Anonymous access for customers (queue joining, status checking)
  - **KioskController**: Streamlined for in-location kiosk interfaces
  - **StaffController**: Administrative features for barbers and staff
  - **OrganizationsController**: Business management for owners
  - **QueuesController**: Core queue operations for all authenticated users

### Future BFF Architecture

The platform is designed to evolve toward a Backend For Frontend (BFF) pattern. Refer to [BFF_ARCHITECTURE.md](./docs/BFF_ARCHITECTURE.md) for the planned future architecture.

### URL Structure

The platform uses a location-centric URL pattern:
```
https://www.eutonafila.com.br/{location-slug}
```

Location slugs are globally unique identifiers across the platform, with rules for:
- ASCII, lowercase formatting
- Hyphenation for spaces
- Optional business type prefixes (barb-, posto-, clin-)

Refer to [URL_STRUCTURE.md](./URL_STRUCTURE.md) for comprehensive URL guidelines.

## 🚀 Quick Start

### Prerequisites

- .NET 8.0 SDK
- MySQL 8.x (local instance or Docker)
- Docker Desktop (optional)
- PowerShell 7+ (for scripts)

### Local Development

```bash
# Clone the repository
git clone <repository-url>
cd backend

# Restore dependencies
dotnet restore GrandeTech.QueueHub/GrandeTech.QueueHub.API/GrandeTech.QueueHub.API.csproj

# Build the project
dotnet build GrandeTech.QueueHub/GrandeTech.QueueHub.API/GrandeTech.QueueHub.API.csproj

# Run tests
dotnet test GrandeTech.QueueHub/GrandeTech.QueueHub.Tests/GrandeTech.QueueHub.Tests.csproj

# Run the API locally
dotnet run --project GrandeTech.QueueHub/GrandeTech.QueueHub.API/GrandeTech.QueueHub.API.csproj
```

### Docker Development

```bash
# Build and run with Docker
docker build -t queuehub-api -f GrandeTech.QueueHub/GrandeTech.QueueHub.API/Dockerfile .
docker run -p 8080:8080 queuehub-api
```

## 🔐 Security Model

The application implements a comprehensive multi-tenant security model with role-based access control:

### Roles

1. **PlatformAdmin** - Platform-level administrator (cross-tenant)
2. **Admin** - Organization administrator (combined Admin + Owner)
3. **Barber** - Staff member at a location
4. **Client** - End user/customer
5. **ServiceAccount** - Background processes/system operations

### Authentication

- JWT-based authentication with tenant-aware claims
- Multi-tenant isolation at organization and location levels
- Role-based authorization with custom policies

Refer to [SECURITY_MODEL.md](./SECURITY_MODEL.md) for detailed security documentation.

## 🏢 Production Environment

### Hosting Information
- **Hosting Provider:** BoaHost
- **Control Panel:** Plesk
- **Database:** MySQL 8.x
- **Domain:** api.eutonafila.com.br
- **SSL:** Enabled with automatic certificates

### Database Configuration

- **MySQL Host:** Provided by BoaHost
- **Database Name:** QueueHubDb
- **Connection String:** Configured via environment variables
- **SSL Mode:** Required in production
- **Auto Migration:** Enabled for seamless updates

### Environment Variables

Production environment variables are configured in Plesk:
- `ASPNETCORE_ENVIRONMENT=Production`
- `MYSQL_HOST`, `MYSQL_DATABASE`, `MYSQL_USER`, `MYSQL_PASSWORD`
- `JWT_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`
- `APPLICATION_INSIGHTS_CONNECTION_STRING`

### Production URLs

- **Core API (Production):** `https://api.eutonafila.com.br`
- **Health Check:** `https://api.eutonafila.com.br/health`
- **Swagger Documentation:** `https://api.eutonafila.com.br/swagger/index.html`

> **Note:** The production API is deployed on BoaHost with MySQL database integration.

## 🔄 CI/CD Pipeline

The project uses GitHub Actions for continuous integration and deployment with automated pipelines:

### Pipeline Features

- **Automated Testing**: Runs all unit tests on every commit
- **Docker Image Building**: Creates versioned container images
- **Azure Container Registry**: Stores and manages container images
- **Azure App Service Deployment**: Automatic deployment to production
- **Health Checks**: Verifies deployment success
- **Rollback Capability**: Easy rollback to previous versions

### Pipeline Structure

```
.github/
  workflows/
    core-api-ci-cd.yml    # Main CI/CD pipeline for Core API
```

### Deployment Process

1. **Build & Test**: Compiles code and runs all tests
2. **Build & Push Image**: Creates Docker image and pushes to ACR
3. **Deploy to Azure**: Updates App Service with new image
4. **Health Check**: Verifies deployment success

> **Deployment & CI/CD:**
> For all up-to-date deployment and CI/CD instructions, see the consolidated guide at [.github/README.md](../.github/README.md).
> For Azure credential management, see [Azure Credentials Management Guide](./markdown/AZURE_CREDENTIALS_MANAGEMENT.md).

## 🏗️ Project Structure

```
GrandeTech.QueueHub/
├── GrandeTech.QueueHub.API/     # Main API project
│   ├── Application/             # Application services and business logic
│   │   ├── Auth/               # Authentication services
│   │   ├── Analytics/          # Analytics and reporting
│   │   ├── Kiosk/             # Kiosk-specific services
│   │   ├── Locations/         # Location management
│   │   ├── Notifications/     # Notification services
│   │   ├── Organizations/     # Organization management
│   │   ├── Promotions/        # Promotions and coupons
│   │   ├── Public/            # Public API services
│   │   ├── QrCode/            # QR code services
│   │   ├── Queues/            # Queue operations
│   │   ├── Services/          # Core services
│   │   ├── ServicesOffered/   # Service management
│   │   ├── Staff/             # Staff management
│   │   └── SubscriptionPlans/ # Subscription management
│   ├── Controllers/            # API controllers
│   ├── Domain/                # Domain models and business rules
│   │   ├── Common/            # Shared domain concepts
│   │   ├── Customers/         # Customer domain
│   │   ├── Locations/         # Location domain
│   │   ├── Notifications/     # Notification domain
│   │   ├── Organizations/     # Organization domain
│   │   ├── Promotions/        # Promotion domain
│   │   ├── Queues/            # Queue domain
│   │   ├── ServicesOffered/   # Services domain
│   │   ├── Staff/             # Staff domain
│   │   ├── Subscriptions/     # Subscription domain
│   │   └── Users/             # User domain
│   ├── Infrastructure/        # Infrastructure concerns
│   │   ├── Authorization/     # Custom authorization
│   │   ├── Data/             # Entity Framework context
│   │   ├── Logging/          # Logging services
│   │   ├── Middleware/       # Custom middleware
│   │   ├── Repositories/     # Data access layer
│   │   └── Services/         # Infrastructure services
│   └── Migrations/           # Database migrations
├── GrandeTech.QueueHub.Tests/  # Unit and integration tests
│   ├── Application/          # Application layer tests
│   ├── Domain/              # Domain layer tests
│   ├── Integration/         # Integration tests
│   └── Infrastructure/      # Infrastructure tests
└── docs/                    # Documentation
```

## 🛠️ Technology Stack

- **Framework**: .NET 8.0
- **Runtime**: ASP.NET Core 8.0
- **Database**: MySQL 8.x (Pomelo.EntityFrameworkCore.MySql)
- **Authentication**: JWT Bearer tokens
- **Documentation**: Swagger/OpenAPI
- **Testing**: MSTest
- **Containerization**: Docker
- **Hosting**: BoaHost (Production)
- **CI/CD**: GitHub Actions
- **Monitoring**: Application Insights

## 📊 API Documentation

The API includes comprehensive Swagger documentation available at:
- **Local**: `https://localhost:5098/swagger` or `https://localhost:7126/swagger`
- **Production**: `https://api.eutonafila.com.br/swagger/index.html`

### Key Endpoints

- **Health Check**: `GET /health`
- **Authentication**: `POST /auth/login`, `POST /auth/register`
- **Organizations**: `POST /organizations`, `GET /organizations`
- **Locations**: `POST /locations`, `GET /locations`
- **Queues**: `POST /queues/join`, `POST /queues/call-next`
- **Staff**: `POST /staff/barbers`, `GET /staff/barbers`

## 🔧 Configuration

### Environment Variables

```json
{
  "ConnectionStrings": {
    "MySqlConnection": "Server=localhost;Database=QueueHubDb;User=root;Password=your_password;Port=3306;CharSet=utf8mb4;SslMode=None;ConnectionTimeout=30;"
  },
  "Jwt": {
    "Key": "your-super-secret-key-with-at-least-32-characters",
    "Issuer": "GrandeTech.QueueHub.API",
    "Audience": "GrandeTech.QueueHub.API"
  },
  "Database": {
    "UseSqlDatabase": true,
    "UseInMemoryDatabase": false,
    "UseBogusRepositories": false,
    "AutoMigrate": true,
    "SeedData": true
  }
}
```

### Docker Configuration

The application is containerized using a multi-stage Dockerfile optimized for:
- Development debugging
- Production deployment
- Security best practices

## 🧪 Testing

### Running Tests

```bash
# Run all tests
dotnet test GrandeTech.QueueHub/GrandeTech.QueueHub.Tests/GrandeTech.QueueHub.Tests.csproj

# Run with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

### Test Structure

- **Unit Tests**: Application services and domain logic
- **Integration Tests**: API controllers and end-to-end scenarios
- **Test Data**: Bogus library for realistic test data generation

## 🚀 Deployment

### Manual Deployment (BoaHost)

```bash
# Build for production
dotnet publish GrandeTech.QueueHub/GrandeTech.QueueHub.API -c Release -o publish

# Upload to BoaHost via Plesk
# Configure environment variables in Plesk:
# - ASPNETCORE_ENVIRONMENT=Production
# - MYSQL_HOST, MYSQL_DATABASE, MYSQL_USER, MYSQL_PASSWORD
# - JWT_KEY, JWT_ISSUER, JWT_AUDIENCE
```

### Automated Deployment

The CI/CD pipeline automatically deploys on merge to `main` branch. For detailed deployment instructions, see:
- [BoaHost Deployment Guide](./docs/BOAHOST_DEPLOYMENT_GUIDE.md)
- [Deployment Guide](./docs/DEPLOYMENT_GUIDE.md)

## 📈 Monitoring and Health Checks

- **Health Endpoint**: `GET /health` - Returns application status
- **Application Insights**: Integrated for production monitoring
- **Logging**: Structured logging with different levels
- **Metrics**: Performance and business metrics collection

## 🔒 Security Considerations

- **Multi-tenant isolation**: Strong separation between organizations
- **JWT security**: Secure token validation and claims processing
- **Role-based access**: Granular permissions based on user roles
- **Input validation**: Comprehensive request validation
- **HTTPS enforcement**: All production traffic encrypted
- **Rate limiting**: Protection against abuse and DDoS attacks
- **Data anonymization**: Privacy protection for sensitive data

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## 📄 License

This project is proprietary software developed by Grande Tech.

## 📞 Support

For support and questions:
- Check the documentation in the `/docs` directory
- Review the API documentation at the Swagger endpoint
- Contact the development team

---

**Last Updated**: January 2025  
**Version**: 1.0.0  
**Status**: Production Ready
