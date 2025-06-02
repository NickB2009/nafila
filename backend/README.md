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
