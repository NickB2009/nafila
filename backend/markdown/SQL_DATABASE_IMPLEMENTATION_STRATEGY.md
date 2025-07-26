# SQL Database Implementation Strategy

## 🎯 Executive Summary

This document outlines a comprehensive strategy for implementing SQL database support in the GrandeTech QueueHub application, with a focus on local testing capabilities before Azure SQL deployment.

## 📋 Current Status

✅ **COMPLETED:**
- Added Entity Framework Core packages
- Created QueueHubDbContext with proper configuration
- Implemented base SQL repository pattern
- Created entity configurations for key domain objects
- Updated dependency injection for flexible repository switching
- Configured multi-environment database settings

🚧 **IN PROGRESS:**
- SQL repository implementations (Organization and Queue completed)

⏳ **PENDING:**
- Database migrations and seeding
- Remaining SQL repository implementations
- Unit of Work pattern
- Azure SQL deployment configuration

## 🏗️ Architecture Overview

### Database Strategy Layers

1. **Domain Layer**: Pure domain entities with business logic
2. **Infrastructure Layer**: EF Core DbContext, repositories, and configurations
3. **Application Layer**: Services that use repositories through interfaces
4. **Presentation Layer**: Controllers that use application services

### Repository Pattern Implementation

```
IRepository<T> (Generic Interface)
├── BogusBaseRepository<T> (In-memory testing)
└── SqlBaseRepository<T> (EF Core implementation)
    ├── SqlOrganizationRepository
    ├── SqlQueueRepository
    ├── SqlLocationRepository (TODO)
    └── ... (Other specific implementations)
```

## 🔧 Configuration Strategy

### Environment-Based Configuration

| Environment | Database | Repository | Use Case |
|-------------|----------|------------|----------|
| **Development** | SQL Server LocalDB | SQL Repositories | Local development with persistent data |
| **Testing** | In-Memory EF Core | SQL Repositories | Integration tests |
| **Staging** | Azure SQL | SQL Repositories | Pre-production testing |
| **Production** | Azure SQL | SQL Repositories | Live environment |
| **Unit Tests** | Bogus (In-memory) | Bogus Repositories | Fast unit testing |

### Configuration Settings

```json
{
  "Database": {
    "UseSqlDatabase": true,      // Use SQL Server/Azure SQL
    "UseInMemoryDatabase": false, // Use EF Core In-Memory for testing
    "UseBogusRepositories": false, // Use Bogus fake data
    "AutoMigrate": true,         // Auto-run migrations on startup
    "SeedData": true            // Seed initial data
  }
}
```

## 📊 Database Schema Design

### Core Tables Structure

```sql
-- Base audit fields for all tables
CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE()
CreatedBy       NVARCHAR(100)
LastModifiedAt  DATETIME2
LastModifiedBy  NVARCHAR(100)
IsDeleted       BIT NOT NULL DEFAULT 0
DeletedAt       DATETIME2
DeletedBy       NVARCHAR(100)
```

### Key Entity Mappings

1. **Organizations** - Multi-tenant support with slug-based routing
2. **Locations** - Physical barbershop locations
3. **Queues** - Daily queue instances per location
4. **QueueEntries** - Individual customer queue positions
5. **Staff** - Barber/staff member management
6. **Customers** - Customer profiles and history

## 🚀 Implementation Phases

### Phase 1: Foundation (COMPLETED)
- [x] EF Core setup and configuration
- [x] Base repository pattern
- [x] Dependency injection configuration
- [x] Entity configurations for core entities

### Phase 2: Core Repositories (IN PROGRESS)
- [x] SqlOrganizationRepository
- [x] SqlQueueRepository
- [ ] SqlLocationRepository
- [ ] SqlCustomerRepository
- [ ] SqlUserRepository
- [ ] SqlStaffMemberRepository

### Phase 3: Advanced Features (PENDING)
- [ ] Unit of Work pattern implementation
- [ ] Database migrations
- [ ] Data seeding logic
- [ ] Performance optimization
- [ ] Bulk operations support

### Phase 4: Testing & Migration (PENDING)
- [ ] Integration tests with SQL repositories
- [ ] Migration from Bogus to SQL repositories
- [ ] Performance benchmarking
- [ ] Data migration scripts

### Phase 5: Azure Deployment (PENDING)
- [ ] Azure SQL Database setup
- [ ] Connection string configuration
- [ ] CI/CD pipeline updates
- [ ] Production data migration

## 🛠️ Local Development Setup

### Prerequisites
- SQL Server LocalDB (installed with Visual Studio)
- .NET 8 SDK
- Entity Framework Core tools

### Setup Steps

1. **Install EF Core Tools:**
   ```bash
   dotnet tool install --global dotnet-ef
   ```

2. **Create Initial Migration:**
   ```bash
   cd backend/GrandeTech.QueueHub/GrandeTech.QueueHub.API
   dotnet ef migrations add InitialCreate
   ```

3. **Update Database:**
   ```bash
   dotnet ef database update
   ```

4. **Run Application:**
   ```bash
   dotnet run
   ```

### Switching Between Repository Types

**For Bogus (In-memory) Testing:**
```json
{
  "Database": {
    "UseBogusRepositories": true
  }
}
```

**For SQL LocalDB:**
```json
{
  "Database": {
    "UseSqlDatabase": true,
    "UseBogusRepositories": false
  }
}
```

**For EF In-Memory (Integration Tests):**
```json
{
  "Database": {
    "UseInMemoryDatabase": true
  }
}
```

## 🧪 Testing Strategy

### Unit Tests
- Use Bogus repositories for fast, isolated tests
- Mock application services
- Focus on business logic testing

### Integration Tests
- Use EF Core In-Memory database
- Test repository implementations
- Test complete application flows

### Performance Tests
- Use SQL Server LocalDB or Azure SQL
- Benchmark repository operations
- Test with realistic data volumes

## 📈 Performance Considerations

### Database Optimization
- **Indexes**: Strategic indexing on frequently queried columns
- **Pagination**: Built-in pagination support in base repository
- **Connection Pooling**: EF Core connection pooling enabled
- **Query Optimization**: LINQ to SQL optimization
- **Bulk Operations**: Batch operations for large data sets

### EF Core Configuration
- **Change Tracking**: Optimized for domain scenarios
- **Lazy Loading**: Disabled by default, explicit loading preferred
- **Query Filters**: Global filters for soft delete
- **Connection Resiliency**: Retry policies for Azure SQL

## 🔒 Security & Compliance

### Data Protection
- **Encryption at Rest**: Azure SQL TDE (Transparent Data Encryption)
- **Encryption in Transit**: SSL/TLS connections
- **Access Control**: Azure AD integration
- **Audit Logging**: Built-in audit trail for all entities

### Compliance Features
- **Soft Delete**: GDPR-compliant data deletion
- **Audit Trail**: Complete change tracking
- **Data Retention**: Configurable retention policies
- **Privacy Controls**: Customer data anonymization

## 🌐 Azure SQL Deployment Strategy

### Azure Resources Required
1. **Azure SQL Database** (Standard/Premium tier)
2. **Azure Key Vault** (connection string storage)
3. **Azure App Service** (application hosting)
4. **Azure Monitor** (logging and monitoring)

### Deployment Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "@Microsoft.KeyVault(SecretUri=https://your-vault.vault.azure.net/secrets/sql-connection-string/)"
  },
  "Database": {
    "UseSqlDatabase": true,
    "AutoMigrate": false,  // Disable in production
    "SeedData": false     // Disable in production
  }
}
```

### Migration Strategy
1. **Schema Migration**: Use EF Core migrations
2. **Data Migration**: Custom migration scripts
3. **Rollback Plan**: Database backup before migration
4. **Validation**: Comprehensive post-migration testing

## 📋 Next Steps & Action Items

### Immediate Tasks (Week 1-2)
1. **Complete remaining SQL repositories**
   - Location, Customer, User, Staff repositories
   - Follow the established pattern from Organization/Queue repos

2. **Create database migrations**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

3. **Implement data seeding**
   - Create development seed data
   - Add seed data configuration

### Short-term Tasks (Week 3-4)
1. **Add Unit of Work pattern**
   - Transaction management
   - Change tracking optimization

2. **Performance optimization**
   - Index analysis and optimization
   - Query performance profiling

3. **Integration testing**
   - Create comprehensive integration tests
   - Test repository switching

### Medium-term Tasks (Month 2)
1. **Azure SQL setup**
   - Provision Azure SQL Database
   - Configure connection strings
   - Set up monitoring

2. **Production migration**
   - Data migration scripts
   - Deployment automation
   - Performance validation

## 🔍 Monitoring & Observability

### Key Metrics to Track
- **Database Performance**: Query execution times, connection pool usage
- **Repository Performance**: Operation latencies, error rates
- **Data Growth**: Table sizes, index effectiveness
- **Business Metrics**: Queue throughput, customer satisfaction

### Logging Strategy
- **EF Core Logging**: SQL query logging in development
- **Application Logging**: Structured logging with Serilog
- **Performance Logging**: Slow query detection
- **Error Logging**: Exception tracking and alerting

## 📚 Documentation & Knowledge Transfer

### Developer Resources
1. **Repository Pattern Guide**: How to implement new repositories
2. **Entity Configuration Guide**: EF Core configuration patterns
3. **Migration Guide**: Database schema changes
4. **Testing Guide**: Repository and integration testing

### Operational Documentation
1. **Deployment Guide**: Azure SQL deployment steps
2. **Monitoring Guide**: Performance monitoring setup
3. **Troubleshooting Guide**: Common issues and solutions
4. **Backup & Recovery**: Data protection procedures

---

## 🎯 Success Criteria

✅ **Technical Success:**
- All repositories implemented with SQL support
- Sub-100ms response times for typical queries
- Zero data loss during migration
- 99.9% uptime in production

✅ **Business Success:**
- Seamless user experience during migration
- Improved application performance
- Enhanced data analytics capabilities
- Scalable foundation for future growth

---

*This strategy provides a solid foundation for implementing SQL database support while maintaining flexibility for testing and ensuring a smooth transition to Azure SQL in production.* 