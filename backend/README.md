# EuToNaFila QueueHub API

A modern, multi-tenant queue management system built with .NET 8, designed for barbershops, clinics, and other service-based businesses.

## 🏗️ Architecture Overview

### Backend For Frontend (BFF) Pattern

The QueueHub/EuToNaFila platform uses a Backend For Frontend (BFF) pattern with:

- **Core API**: Centralized backend with business logic, data models, and multi-tenant support
- **Multiple BFF Services**: Specialized backends for different frontend types:
  - Barbershop Web BFF: Optimized for desktop with administrative features
  - Clinic Web BFF: Healthcare domain with clinic-specific features
  - Kiosk BFF: Streamlined for in-location interfaces

Refer to [BFF_ARCHITECTURE.md](./BFF_ARCHITECTURE.md) for detailed architecture information.

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
- Docker Desktop
- Azure CLI (for deployment)
- PowerShell 7+ (for scripts)

### Local Development

```bash
# Clone the repository
git clone <repository-url>
cd backend

# Restore dependencies
dotnet restore GrandeTech.QueueHub.API/GrandeTech.QueueHub.API/GrandeTech.QueueHub.API.csproj

# Build the project
dotnet build GrandeTech.QueueHub.API/GrandeTech.QueueHub.API/GrandeTech.QueueHub.API.csproj

# Run tests
dotnet test GrandeTech.QueueHub.API/GrandeTech.QueueHub.Tests/GrandeTech.QueueHub.Tests.csproj

# Run the API locally
dotnet run --project GrandeTech.QueueHub.API/GrandeTech.QueueHub.API/GrandeTech.QueueHub.API.csproj
```

### Docker Development

```bash
# Build and run with Docker
docker build -t queuehub-api -f GrandeTech.QueueHub.API/GrandeTech.QueueHub.API/Dockerfile .
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

## 🏢 Azure Environment

### Subscription Information
- **Subscription Name:** Grande Tech
- **Subscription ID:** `160af2e4-319f-4384-b38f-71e23d140a0f`
- **Tenant ID:** `621db766-1ccd-40f1-80b0-ef469bd8f081`
- **Directory:** Default Directory (rommelbgmail.onmicrosoft.com)
- **Status:** Active
- **Role:** Owner

### Current Resources

- **Resource Group:** `rg-p-queuehub-core-001` (Production, Core Services)
- **App Service:** `app-p-queuehub-api-001` (Core API)
- **App Service Plan:** `asp-p-queuehub-core-001` (F1: 1)
- **Primary Region:** Brazil South
- **Container Registry:** `acrqueuehubapi001.azurecr.io`

### Production URLs

- **Core API (Production):** `https://app-p-queuehub-api-001-e4g8b2e8dngffgc5.brazilsouth-01.azurewebsites.net`
- **Health Check:** `https://app-p-queuehub-api-001-e4g8b2e8dngffgc5.brazilsouth-01.azurewebsites.net/health`
- **Swagger Documentation:** `https://app-p-queuehub-api-001-e4g8b2e8dngffgc5.brazilsouth-01.azurewebsites.net/swagger`

> **Note:** The production URL includes a unique identifier (`e4g8b2e8dngffgc5`) which is automatically generated by Azure App Service for security and routing purposes.

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
GrandeTech.QueueHub.API/
├── Application/           # Application services and business logic
│   ├── Auth/             # Authentication services
│   ├── Locations/        # Location management
│   ├── Organizations/    # Organization management
│   ├── Queues/          # Queue operations
│   ├── ServicesOffered/ # Service management
│   └── Staff/           # Staff management
├── Controllers/          # API controllers
├── Domain/              # Domain models and business rules
│   ├── Common/          # Shared domain concepts
│   ├── Customers/       # Customer domain
│   ├── Locations/       # Location domain
│   ├── Organizations/   # Organization domain
│   ├── Queues/          # Queue domain
│   ├── ServicesOffered/ # Services domain
│   ├── Staff/           # Staff domain
│   └── Users/           # User domain
├── Infrastructure/      # Infrastructure concerns
│   ├── Authorization/   # Custom authorization
│   ├── Persistence/     # Data persistence
│   └── Repositories/    # Data access layer
└── GrandeTech.QueueHub.Tests/  # Unit and integration tests
```

## 🛠️ Technology Stack

- **Framework**: .NET 8.0
- **Runtime**: ASP.NET Core 8.0
- **Authentication**: JWT Bearer tokens
- **Documentation**: Swagger/OpenAPI
- **Testing**: MSTest
- **Containerization**: Docker
- **Cloud Platform**: Azure
- **CI/CD**: GitHub Actions
- **Container Registry**: Azure Container Registry

## 📊 API Documentation

The API includes comprehensive Swagger documentation available at:
- **Local**: `https://localhost:8080/swagger`
- **Production**: `https://app-p-queuehub-api-001-e4g8b2e8dngffgc5.brazilsouth-01.azurewebsites.net/swagger`

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
    "DefaultConnection": "Server=...;Database=GrandeTechQueueHub;..."
  },
  "Jwt": {
    "Key": "your-super-secret-key-with-at-least-32-characters",
    "Issuer": "GrandeTech.QueueHub.API",
    "Audience": "GrandeTech.QueueHub.API"
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
dotnet test GrandeTech.QueueHub.API/GrandeTech.QueueHub.Tests/GrandeTech.QueueHub.Tests.csproj

# Run with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

### Test Structure

- **Unit Tests**: Application services and domain logic
- **Integration Tests**: API controllers and end-to-end scenarios
- **Test Data**: Bogus library for realistic test data generation

## 🚀 Deployment

### Manual Deployment

```powershell
# Build and push Docker image
docker build -t acrqueuehubapi001.azurecr.io/queuehub-api:v1 -f GrandeTech.QueueHub.API/GrandeTech.QueueHub.API/Dockerfile .
az acr login --name acrqueuehubapi001
docker push acrqueuehubapi001.azurecr.io/queuehub-api:v1

# Deploy to Azure App Service
az webapp config container set --name app-p-queuehub-api-001 --resource-group rg-p-queuehub-core-001 --container-image-name acrqueuehubapi001.azurecr.io/queuehub-api:v1
az webapp restart --name app-p-queuehub-api-001 --resource-group rg-p-queuehub-core-001
```

### Automated Deployment

The CI/CD pipeline automatically deploys on merge to `main` branch. See [.github/README.md](../../.github/README.md) for setup and troubleshooting instructions.

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

**Last Updated**: December 2024  
**Version**: 1.0.0  
**Status**: Production Ready
