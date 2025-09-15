# SQL Database Implementation Strategy

## 🎯 Executive Summary

This document describes how the QueueHub backend uses SQL Server with Entity Framework Core in local Docker and cloud (Azure SQL) environments. It reflects the current Docker‑based development setup.

## 📋 Current Status

✅ **COMPLETED:**
- EF Core setup and DbContext
- Repository abstraction with SQL and Bogus implementations
- SQL Server 2022 containerized in Docker with healthcheck
- **Azure SQL Database integration in production** ✅
- Connection string binding via `ConnectionStrings:AzureSqlConnection`
- Public API endpoints working with database (e.g., GET /api/Public/salons)

🚧 **IN PROGRESS:**
- Additional SQL repositories beyond Organization/Queue

⏳ **PENDING:**
- Migrations and seeding pipeline stabilization
- Docker-compose password consistency fixes

## 🏗️ Architecture Overview

### Layers
1. Domain (entities, rules)
2. Infrastructure (EF Core, repositories, configurations)
3. Application (services)
4. API (controllers/endpoints)

### Repository Pattern (overview)
```
IRepository<T>
├─ BogusBaseRepository<T> (tests/dev fake)
└─ SqlBaseRepository<T> (EF Core)
   ├─ SqlOrganizationRepository
   ├─ SqlQueueRepository
   ├─ ...
```

## 🔧 Configuration Strategy

### Environment Settings
```json
{
  "Database": {
    "UseSqlDatabase": true,
    "UseInMemoryDatabase": false,
    "UseBogusRepositories": false,
    "AutoMigrate": true,
    "SeedData": true
  }
}
```

### Connection String Keys
- App uses `ConnectionStrings:AzureSqlConnection`.
- In Docker, this is set via env var `ConnectionStrings__AzureSqlConnection`.

### Docker Compose Example
```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    ports:
      - "1434:1433"
  queuehub-api:
    ports:
      - "8080:80"
    environment:
      - ConnectionStrings__AzureSqlConnection=Server=sqlserver:1433;Database=QueueHubDb;User Id=sa;Password=DevPassword123!;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;
```

## 📊 Schema and Conventions
- Common audit fields on all tables (CreatedAt, LastModifiedAt, etc.)
- Soft delete via `IsDeleted` and `DeletedAt`

## 🚀 Implementation Phases
- Phase 1: EF Core + base repos ✅
- Phase 2: Remaining SQL repos ◻︎
- Phase 3: Migrations + seeding ◻︎
- Phase 4: Integration tests with SQL ◻︎
- Phase 5: Azure SQL rollout ◻︎

## 🛠️ Local Development

### Prerequisites
- Docker Desktop
- .NET 8 SDK
- EF Core Tools (`dotnet tool install --global dotnet-ef`)

### Running locally with Docker
```powershell
cd GrandeTech.QueueHub
docker-compose up -d --build
# API → http://localhost:8080
```

### Migrations (example)
```powershell
cd GrandeTech.QueueHub/GrandeTech.QueueHub.API
# Create or update schema
# dotnet ef migrations add InitialCreate
# dotnet ef database update
```

## 🧪 Testing Strategy
- Unit tests: Bogus repositories
- Integration tests: in‑memory or Docker SQL (preferred for E2E)

## 📈 Performance
- Indexing, pagination, pooling, and batching where appropriate

## 🔒 Security
- Secrets via environment variables locally
- Use Azure Key Vault in cloud
- TLS enforced in Azure SQL (disable AutoMigrate/Seed in prod)

## 🌐 Azure SQL
- **✅ PRODUCTION WORKING**: Azure SQL Database successfully integrated
- Connection string format: `Server=tcp:grande.database.windows.net,1433;Initial Catalog=barberqueue;User ID=CloudSA24b045fd;Password=****;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;`
- Store connection string in Azure App Service Configuration (not Key Vault for this setup)
- CI/CD injects `ConnectionStrings__AzureSqlConnection`

---
This strategy reflects the current Dockerized SQL setup and prepares for Azure SQL migration. 