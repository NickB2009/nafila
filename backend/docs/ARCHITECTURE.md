# EuTôNaFila / QueueHub - Architecture Documentation

## Overview

The EuTôNaFila queue management platform uses a modern, scalable architecture designed to support multiple business domains (barbershops, health clinics, etc.) across various frontend types (web, mobile, kiosk).

## Architecture Patterns

### Backend For Frontend (BFF)

The platform implements a BFF pattern to optimize each frontend type while maintaining a single core backend service.

#### Core Components

| Component | Purpose |
|-----------|---------|
| **Core Backend API** | Houses business logic, data models, and domain services |
| **Barbershop Web BFF** | Optimized for desktop web browsers with administrative features |
| **Barbershop Mobile BFF** | Optimized for mobile apps with barber-specific features |
| **Clinic Web BFF** | Specialized for healthcare domain with clinic-specific features |
| **Clinic Mobile BFF** | Mobile-optimized for healthcare with patient-focused features |
| **Kiosk BFF** | Streamlined for in-location kiosk interfaces |

#### BFF Responsibilities

1. **Data Transformation**
   - Reshape data for specific frontend requirements
   - Aggregate multiple backend API calls into single client responses
   - Filter unnecessary data to reduce payload sizes

2. **Authentication Flows**
   - Implement frontend-specific authentication methods
   - Manage token exchange and refresh processes
   - Handle user session management appropriate to the client type

3. **Optimization**
   - Response compression and formatting for target devices
   - Implement client-specific caching strategies
   - Batch requests to reduce network overhead

4. **API Versioning**
   - Manage client-specific API versioning
   - Provide backward compatibility for older client versions
   - Facilitate gradual client migration during updates

## URL Structure

### Core Philosophy

EuTôNaFila uses a location-centric URL structure that prioritizes:
- Brevity and simplicity
- Global uniqueness
- Human readability
- QR code friendliness

### URL Pattern

```
https://www.eutonafila.com.br/{location-slug}
```

### Location Slug Rules

| Rule | Details |
|------|---------|
| **location-slug** | Single, lower-case, ASCII token that uniquely identifies the physical site |
| **Types/prefixes** (optional) | Descriptive prefix ("barb", "posto", "clin") for human readability |
| **Branch numbering** | Append "-2", "-3" for colliding slugs |

### URL Examples

| Location Name | Suggested Slug | Final URL |
|---------------|----------------|-----------|
| Barbearia Mineiro (Morro Grande) | `mineiro-morrogrande` | https://www.eutonafila.com.br/mineiro-morrogrande |
| Barbearia Mineiro (Morro da Fumaça) | `mineiro-morrodafumaca` | https://www.eutonafila.com.br/mineiro-morrodafumaca |
| Posto de Saúde Morro Grande (1ª unidade) | `posto-morrogrande-1` | https://www.eutonafila.com.br/posto-morrogrande-1 |

### Extended URL Patterns

```bash
# Client Queue View
https://www.eutonafila.com.br/{location-slug}/queue

# Admin Views
https://www.eutonafila.com.br/{location-slug}/admin

# Kiosk Mode
https://kiosk.eutonafila.com.br/{location-slug}

# Mobile Deep Links
eutonafila://{location-slug}
eutonafila://{location-slug}/join
```

## Database Architecture

### MySQL Optimization

#### Data Type Mappings

| SQL Server Type | MySQL Type | Notes | Example |
|----------------|------------|-------|---------|
| `uniqueidentifier` | `CHAR(36)` | GUID as string | `550e8400-e29b-41d4-a716-446655440000` |
| `nvarchar(max)` | `LONGTEXT` | Large text | `LONGTEXT CHARACTER SET utf8mb4` |
| `varbinary(max)` | `LONGBLOB` | Binary data | `LONGBLOB` |
| `datetime2` | `DATETIME(6)` | High precision datetime | `2024-01-15 14:30:25.123456` |
| `bit` | `TINYINT(1)` | Boolean | `0` or `1` |
| `rowversion` | `TIMESTAMP` | Auto-updating timestamp | `CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP` |
| `decimal(18,2)` | `DECIMAL(18,2)` | Same precision | `123.45` |

#### Character Set and Collation
- **Character Set**: `utf8mb4` (supports full Unicode including emojis)
- **Collation**: `utf8mb4_unicode_ci` (case-insensitive Unicode)

#### Performance Optimizations
1. **JSON Columns**: Use native `JSON` type instead of `LONGTEXT`
2. **Indexes**: Create composite indexes for common query patterns
3. **Connection Pooling**: Configure appropriate pool sizes
4. **Query Optimization**: Use EXPLAIN to analyze query performance

### Entity Flattening Strategy

#### Goals
- Eliminate complex value object mappings that cause EF Core runtime exceptions
- Replace with simple, flat column mappings for better stability and performance
- Maintain backward compatibility and zero downtime

#### Flattening Scope

**Phase 1 (High Priority)**:
- `Location` entity: Flatten `CustomBranding` and `WeeklyHours`
- `ServiceOffered` entity: Flatten `Price` value object
- `SubscriptionPlan` entity: Flatten `Price` value object

**Phase 2 (Medium Priority)**:
- `Organization` entity: Flatten `CustomBranding`
- `User` entity: Flatten `Email` and `PhoneNumber` value objects
- `Customer` entity: Flatten `Email` and `PhoneNumber` value objects

#### Example: Location Entity Flattening

**Before (Value Objects)**:
```csharp
public class Location : Entity
{
    public CustomBranding CustomBranding { get; set; }
    public WeeklyBusinessHours WeeklyHours { get; set; }
}
```

**After (Flattened)**:
```csharp
public class Location : Entity
{
    // Branding fields
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? LogoUrl { get; set; }
    public string? FontFamily { get; set; }
    
    // Weekly hours fields
    public TimeSpan? MondayOpenTime { get; set; }
    public TimeSpan? MondayCloseTime { get; set; }
    public bool MondayIsClosed { get; set; }
    // ... repeat for each day
}
```

## Security Architecture

### Role-Based Security Model

#### Consolidated Roles
1. **PlatformAdmin** - Platform-level administrator (cross-tenant)
2. **Admin** - Organization administrator (combined Admin + Owner)
3. **Barber** - Staff member at a location
4. **Client** - End user/customer
5. **ServiceAccount** - Background processes/system operations

#### Authorization Attributes
```csharp
[RequirePlatformAdmin]  // Cross-organization operations
[RequireAdmin]         // Organization-level operations
[RequireBarber]        // Location-level operations
[RequireClient]        // Authenticated user operations
[AllowPublicAccess]    // No authentication required
[RequireServiceAccount] // Background processes
```

#### Tenant-Aware Claims Structure
```csharp
// Standard claims
sub: user_id
role: PlatformAdmin|Admin|Barber|Client|ServiceAccount
email: user@example.com

// Tenant-specific claims
org_id: organization_guid
loc_id: location_guid (for location-scoped users)
tenant_slug: location-slug
permissions: ["read:queue", "write:staff"]
is_service_account: true|false
```

### Security Implementation
- **JWT Authentication**: Token-based authentication for all protected routes
- **Tenant Isolation**: Strong separation between organizations and locations
- **Role-Based Access**: Hierarchical permission system
- **Public Endpoints**: Clear separation of public vs private operations

## Kiosk Display Architecture

### Real-Time Updates Strategy

#### Polling Approach (Current)
- **Pros**: Simple implementation, works with existing RESTful architecture
- **Cons**: Higher server load, increased network traffic, potential stale data

#### WebSocket Approach (Future)
- **Pros**: Reduced server loads, low-latency updates, efficient resource utilization
- **Cons**: Requires WebSocket infrastructure, connection management complexity

#### Recommended Optimizations
1. **Smart Polling**: Detect display state, adjust frequency based on activity
2. **Visual Enhancements**: Progress animation, smooth transitions
3. **Efficient Data Transfer**: Minimal payload, compression, ETag headers

### Kiosk Display Best Practices
- **Polling Strategy**: Poll every 5-10 seconds for updates
- **Visual Design**: Hide cursor, disable text selection, remove scrollbars
- **Error Handling**: Implement exponential backoff for failed requests
- **Performance**: Use lightweight data transfer, minimal payload

### CSS for Kiosk Displays
```css
/* Hide cursor for kiosk displays */
body { cursor: none; }

/* Disable text selection */
* {
    -webkit-user-select: none;
    -moz-user-select: none;
    -ms-user-select: none;
    user-select: none;
}

/* Hide scrollbars */
::-webkit-scrollbar { display: none; }
body { -ms-overflow-style: none; scrollbar-width: none; }
```

## Technical Stack

### Core Technologies
- **Backend**: ASP.NET Core 8.0 API
- **Database**: MySQL 8.x
- **Authentication**: JWT-based authentication
- **Hosting**: BoaHost with Plesk control panel
- **Frontend**: Flutter for mobile, web technologies for kiosk displays

### Development Tools
- **IDE**: Visual Studio / VS Code with Cursor
- **Testing**: MSTest with WebApplicationFactory
- **Version Control**: Git
- **CI/CD**: PowerShell scripts for deployment

### Monitoring and Logging
- **Application Insights**: Azure-based telemetry (optional)
- **BoaHost Native Logging**: File-based logging for production
- **Health Checks**: Built-in health monitoring endpoints
- **Performance Metrics**: System resource monitoring

## Deployment Architecture

### BoaHost Production Setup
- **Application Hosting**: Plesk-managed .NET application
- **Database**: BoaHost MySQL service
- **SSL/TLS**: Let's Encrypt or custom certificates
- **Domain Management**: DNS configuration via BoaHost
- **Monitoring**: BoaHost native logging and metrics

### Standalone Server Option
- **Application**: Self-contained .NET application
- **Service Management**: Systemd service configuration
- **Database**: External MySQL connection
- **Monitoring**: Systemd journal and custom logging

## API Design Patterns

### RESTful API Structure
- **Resource-Based URLs**: `/api/{resource}/{id}`
- **HTTP Methods**: GET, POST, PUT, DELETE for CRUD operations
- **Status Codes**: Standard HTTP status codes for responses
- **Error Handling**: Consistent error response format

### DTO Pattern
- **Never return domain entities directly from controllers**
- **Always use DTOs (Data Transfer Objects) for API input and output**
- **Map between domain models and DTOs in controllers or dedicated mapping layer**
- **DTOs designed for API contract, not internal persistence**

### API Versioning
- **URL Versioning**: `/api/v1/{resource}`
- **Header Versioning**: `Accept: application/vnd.api+json;version=1`
- **Backward Compatibility**: Maintain support for previous versions

## Performance Considerations

### Database Optimization
- **Indexing Strategy**: Composite indexes for common query patterns
- **Connection Pooling**: Optimized pool sizes for MySQL
- **Query Optimization**: Use EXPLAIN to analyze and optimize queries
- **Caching**: In-memory caching for frequently accessed data

### Application Performance
- **Response Compression**: Gzip compression for API responses
- **Async Operations**: Async/await patterns throughout the application
- **Resource Management**: Proper disposal of database connections and resources
- **Memory Management**: Efficient object lifecycle management

### Scalability Considerations
- **Horizontal Scaling**: Stateless application design
- **Load Balancing**: Multiple application instances behind load balancer
- **Database Scaling**: Read replicas for read-heavy operations
- **Caching Strategy**: Distributed caching for high-traffic scenarios

## Future Architecture Considerations

### Microservices Migration
- **Service Decomposition**: Break monolith into domain-specific services
- **API Gateway**: Centralized routing and cross-cutting concerns
- **Service Mesh**: Service-to-service communication management
- **Event-Driven Architecture**: Asynchronous communication between services

### Cloud Migration
- **Container Orchestration**: Kubernetes for container management
- **Serverless Functions**: Azure Functions for event-driven processing
- **Managed Services**: Azure SQL, Azure Redis Cache, Azure Service Bus
- **DevOps Pipeline**: CI/CD with Azure DevOps or GitHub Actions

---

*Last updated: 2025-01-15*



