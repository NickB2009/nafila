# EuToNaFila Azure Setup

## Azure Environment

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

## Resource Provisioning

### Resource Groups

#### Core Services
```powershell
az group create --name rg-p-queuehub-core-001 --location brazilsouth --tags Project=EuToNaFila Environment=Production CreatedBy=DevTeam
```

#### BFF Services (Frontend-specific backends)
```powershell
# Barbershop Web BFF
az group create --name rg-p-queuehub-bff-barbershop-web-001 --location brazilsouth --tags Project=EuToNaFila Environment=Production CreatedBy=DevTeam Service=BFF-Barbershop-Web

# Clinic Web BFF
az group create --name rg-p-queuehub-bff-clinic-web-001 --location brazilsouth --tags Project=EuToNaFila Environment=Production CreatedBy=DevTeam Service=BFF-Clinic-Web

# Kiosk BFF
az group create --name rg-p-queuehub-bff-kiosk-001 --location brazilsouth --tags Project=EuToNaFila Environment=Production CreatedBy=DevTeam Service=BFF-Kiosk
```

## App Service Deployment

### Core API App Service
```powershell
# Create App Service Plan
az appservice plan create --name asp-p-queuehub-core-001 --resource-group rg-p-queuehub-core-001 --sku S1 --is-linux false --tags Project=EuToNaFila Environment=Production CreatedBy=DevTeam

# Create Core API App Service
az webapp create --name app-p-queuehub-api-001 --resource-group rg-p-queuehub-core-001 --plan asp-p-queuehub-core-001 --runtime "DOTNET|8.0" --tags Project=EuToNaFila Environment=Production CreatedBy=DevTeam Service=CoreAPI

# Enable managed identity
az webapp identity assign --name app-p-queuehub-api-001 --resource-group rg-p-queuehub-core-001
```

### BFF App Services
```powershell
# Barbershop Web BFF
az webapp create --name app-p-queuehub-bff-barbershop-web-001 --resource-group rg-p-queuehub-bff-barbershop-web-001 --plan asp-p-queuehub-core-001 --runtime "DOTNET|8.0" --tags Project=EuToNaFila Environment=Production CreatedBy=DevTeam Service=BFF-Barbershop-Web

# Clinic Web BFF
az webapp create --name app-p-queuehub-bff-clinic-web-001 --resource-group rg-p-queuehub-bff-clinic-web-001 --plan asp-p-queuehub-core-001 --runtime "DOTNET|8.0" --tags Project=EuToNaFila Environment=Production CreatedBy=DevTeam Service=BFF-Clinic-Web

# Kiosk BFF
az webapp create --name app-p-queuehub-bff-kiosk-001 --resource-group rg-p-queuehub-bff-kiosk-001 --plan asp-p-queuehub-core-001 --runtime "DOTNET|8.0" --tags Project=EuToNaFila Environment=Production CreatedBy=DevTeam Service=BFF-Kiosk
```

## Setting Up Monitoring

```powershell
# Create Application Insights for Core API
az monitor app-insights component create --app app-insights-p-queuehub-core-001 --location brazilsouth --resource-group rg-p-queuehub-core-001 --application-type web --tags Project=EuToNaFila Environment=Production CreatedBy=DevTeam

# Connect Application Insights to Core API
az webapp config appsettings set --name app-p-queuehub-api-001 --resource-group rg-p-queuehub-core-001 --settings APPINSIGHTS_INSTRUMENTATIONKEY=$(az monitor app-insights component show --app app-insights-p-queuehub-core-001 --resource-group rg-p-queuehub-core-001 --query instrumentationKey --output tsv) ApplicationInsightsAgent_EXTENSION_VERSION=~2
```

## Security Configuration

### Key Vault Setup
```powershell
# Create Key Vault
az keyvault create --name kv-p-queuehub-core-001 --resource-group rg-p-queuehub-core-001 --location brazilsouth --tags Project=EuToNaFila Environment=Production CreatedBy=DevTeam

# Grant Core API Managed Identity access to Key Vault
az keyvault set-policy --name kv-p-queuehub-core-001 --object-id $(az webapp identity show --name app-p-queuehub-api-001 --resource-group rg-p-queuehub-core-001 --query principalId --output tsv) --secret-permissions get list
```

## Database Configuration

```powershell
# Create Azure SQL Server
az sql server create --name sql-p-queuehub-core-001 --resource-group rg-p-queuehub-core-001 --location brazilsouth --admin-user sqladmin --admin-password "ComplexP@ssword123" --tags Project=EuToNaFila Environment=Production CreatedBy=DevTeam

# Create Database
az sql db create --name sqldb-p-queuehub-core-001 --resource-group rg-p-queuehub-core-001 --server sql-p-queuehub-core-001 --service-objective S1 --tags Project=EuToNaFila Environment=Production CreatedBy=DevTeam
```

## Infrastructure as Code

The EuToNaFila project uses Bicep to manage Azure resources. Template files are stored in the `/infra` directory.

### Core Infrastructure Deployment

```powershell
# Deploy core resources using Bicep templates
az deployment group create --resource-group rg-p-queuehub-core-001 --template-file ./infra/main.bicep --parameters environment=p projectName=queuehub
```

## CI/CD Pipeline

The project uses GitHub Actions for continuous integration and deployment with separate pipelines for:
- Core API backend
- Each BFF service

### Pipeline Structure

```
.github/
  workflows/
    core-api-ci-cd.yml
    bff-barbershop-web-ci-cd.yml
    bff-clinic-web-ci-cd.yml
    bff-kiosk-ci-cd.yml
```

## Cost Management

### Budget Setup

```powershell
# Set budget alert for Core Resource Group
az consumption budget create --name "CoreBudget" \
  --resource-group rg-p-queuehub-core-001 \
  --amount 100 \
  --time-grain monthly \
  --start-date $(date +%Y-%m-01) \
  --end-date 2026-12-31 \
  --category cost \
  --notification threshold=80 threshold=100 \
  --notification-enabled true
```

## Upgrade Considerations

The current App Service Plan is on F1 (Free) tier, which has limitations:
- 60 minutes of compute per day
- No custom domain support
- No SSL support
- No auto-scaling

For production workloads, consider upgrading to at least S1 tier:

```powershell
# Upgrade App Service Plan to Standard tier
az appservice plan update --name asp-p-queuehub-core-001 --resource-group rg-p-queuehub-core-001 --sku S1
```

## Architecture Documentation

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
