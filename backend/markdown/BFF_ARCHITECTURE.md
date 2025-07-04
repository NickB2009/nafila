# Backend For Frontend (BFF) Architecture

## Overview

The QueueHub/EuToNaFila platform uses a Backend For Frontend (BFF) architecture pattern to support multiple frontends while maintaining a single core backend service. This document outlines the BFF architecture design principles, components, and implementation guidelines.

> **Note:** For URL structure details, see [URL_STRUCTURE.md](./URL_STRUCTURE.md)

## Architecture Components

### Core Backend API

- Centralized backend that serves all client types
- Houses core business logic, data models, and domain services
- Implements RESTful API endpoints with consistent patterns
- Handles database interactions, caching, and business workflows
- Supports multi-tenancy for different business domains (barbershops, clinics, etc.)
- Provides location-based routing via globally unique location slugs

### BFF Services

Dedicated BFF services for each frontend type:

| BFF Service | Purpose |
|-------------|---------|
| Barbershop Web BFF | Optimized for desktop web browsers with administrative features |
| Barbershop Mobile BFF | Optimized for mobile apps with barber-specific features |
| Clinic Web BFF | Specialized for healthcare domain with clinic-specific features |
| Clinic Mobile BFF | Mobile-optimized for healthcare with patient-focused features |
| Kiosk BFF | Streamlined for in-location kiosk interfaces |

## BFF Responsibilities

Each BFF layer is responsible for:

1. **Data Transformation**
   - Reshaping data for specific frontend requirements
   - Aggregating multiple backend API calls into single client responses
   - Filtering unnecessary data to reduce payload sizes

2. **Authentication Flows**
   - Implementing frontend-specific authentication methods
   - Managing token exchange and refresh processes
   - Handling user session management appropriate to the client type

3. **Optimization**
   - Response compression and formatting for target devices
   - Implementing client-specific caching strategies
   - Batching requests to reduce network overhead

4. **API Versioning**
   - Managing client-specific API versioning
   - Providing backward compatibility for older client versions
   - Facilitating gradual client migration during updates

## Implementation Guidelines

### URL Structure

- Use location-slug based routing for all public-facing URLs
- Format: `https://www.eutonafila.com.br/{location-slug}`
- Location slugs are globally unique across the platform
- Organization information is stored in the database, not in URLs
- See [URL_STRUCTURE.md](./URL_STRUCTURE.md) for complete details

### API Design

- Use GraphQL for flexible data retrieval in BFF layers
- Maintain RESTful APIs in the core backend with consistent patterns
- Document all APIs with OpenAPI/Swagger specifications
- Implement proper pagination, filtering, and sorting in all APIs
- Support location-slug based routing across all APIs (see [URL_STRUCTURE.md](./URL_STRUCTURE.md))

### Deployment Model

- Deploy core backend and BFFs as separate services
- Use containerization for consistent deployment across environments
- Implement CI/CD pipelines for each service with appropriate testing
- Configure auto-scaling based on each frontend's usage patterns

### Security

- Implement JWT-based authentication between services
- Use managed identities for Azure service authentication
- Keep secrets in Azure Key Vault
- Implement appropriate CORS policies for web clients
- Apply principle of least privilege for all service-to-service communication

### Error Handling

- Implement consistent error formats across all BFFs
- Add client-specific error messages and handling
- Log errors with appropriate context for troubleshooting
- Provide graceful degradation in case of partial system failures

### Monitoring

- Use Application Insights for telemetry
- Implement distributed tracing across BFF and backend services
- Set up dashboards for monitoring each BFF's performance
- Configure alerts for critical service degradation

## Technical Stack

- **Core Backend**: ASP.NET Core API
- **BFF Services**: ASP.NET Core API with GraphQL endpoints
- **Authentication**: Azure AD B2C / Custom JWT implementation
- **API Gateway**: Azure API Management
- **Hosting**: Azure App Service or Azure Container Apps
- **Database**: Azure SQL Database with multi-tenant configuration

## Location Slug Management

The system automatically generates location slugs following these rules:
1. Convert location name to ASCII and lowercase
2. Replace spaces/special characters with hyphens
3. Check for global uniqueness
4. Append numerical suffix if needed (e.g., "-2", "-3")

This approach ensures:
- Short, memorable URLs
- QR code friendliness
- Consistent routing across all frontend types
- Separation of URL structure from organization hierarchy
