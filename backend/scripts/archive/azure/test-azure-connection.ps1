# Test Azure Connection and Get Subscription Information
# This script verifies Azure CLI connectivity and retrieves subscription details

Write-Host "Testing Azure Connection for QueueHub API..." -ForegroundColor Green

# Check if Azure CLI is installed
Write-Host "`n=== Checking Azure CLI Installation ===" -ForegroundColor Cyan
try {
    $azVersion = az version --output json | ConvertFrom-Json
    Write-Host "Azure CLI Version: $($azVersion.'azure-cli')" -ForegroundColor Green
    Write-Host "Azure CLI Core Version: $($azVersion.'azure-cli-core')" -ForegroundColor Green
} catch {
    Write-Host "Error: Azure CLI is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install Azure CLI from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Yellow
    exit 1
}

# Check if logged in to Azure
Write-Host "`n=== Checking Azure Authentication ===" -ForegroundColor Cyan
try {
    $account = az account show --output json | ConvertFrom-Json
    Write-Host "Logged in as: $($account.user.name)" -ForegroundColor Green
    Write-Host "Subscription ID: $($account.id)" -ForegroundColor Green
    Write-Host "Subscription Name: $($account.name)" -ForegroundColor Green
    Write-Host "Tenant ID: $($account.tenantId)" -ForegroundColor Green
} catch {
    Write-Host "Error: Not logged in to Azure" -ForegroundColor Red
    Write-Host "Please run: az login" -ForegroundColor Yellow
    exit 1
}

# List all subscriptions
Write-Host "`n=== Available Subscriptions ===" -ForegroundColor Cyan
try {
    $subscriptions = az account list --output json | ConvertFrom-Json
    foreach ($sub in $subscriptions) {
        $state = if ($sub.state -eq "Enabled") { "✓" } else { "✗" }
        Write-Host "$state $($sub.name) ($($sub.id)) - $($sub.state)" -ForegroundColor $(if ($sub.state -eq "Enabled") { "Green" } else { "Red" })
    }
} catch {
    Write-Host "Error: Failed to list subscriptions" -ForegroundColor Red
    exit 1
}

# Check for QueueHub resources
Write-Host "`n=== Checking QueueHub Resources ===" -ForegroundColor Cyan
$resourceGroups = @("rg-p-queuehub-core-001", "rg-d-queuehub-core-001")
$foundResources = @()

foreach ($rg in $resourceGroups) {
    try {
        $rgExists = az group show --name $rg --query "name" --output tsv 2>$null
        if ($rgExists) {
            Write-Host "✓ Resource Group found: $rg" -ForegroundColor Green
            $foundResources += $rg
            
            # Check for App Service
            $appServices = az webapp list --resource-group $rg --query "[].name" --output tsv 2>$null
            if ($appServices) {
                Write-Host "  App Services: $appServices" -ForegroundColor Cyan
            }
            
            # Check for ACR
            $acrs = az acr list --resource-group $rg --query "[].name" --output tsv 2>$null
            if ($acrs) {
                Write-Host "  Container Registries: $acrs" -ForegroundColor Cyan
            }
            
            # Check for Application Insights
            $appInsights = az monitor app-insights component list --resource-group $rg --query "[].name" --output tsv 2>$null
            if ($appInsights) {
                Write-Host "  Application Insights: $appInsights" -ForegroundColor Cyan
            }
        } else {
            Write-Host "✗ Resource Group not found: $rg" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "✗ Error checking Resource Group: $rg" -ForegroundColor Red
    }
}

# Test specific App Service connectivity
Write-Host "`n=== Testing App Service Connectivity ===" -ForegroundColor Cyan
$appServiceName = "app-p-queuehub-api-001"
$resourceGroup = "rg-p-queuehub-core-001"

try {
    $appService = az webapp show --name $appServiceName --resource-group $resourceGroup --output json 2>$null | ConvertFrom-Json
    if ($appService) {
        Write-Host "✓ App Service found: $($appService.name)" -ForegroundColor Green
        Write-Host "  State: $($appService.state)" -ForegroundColor Cyan
        Write-Host "  URL: $($appService.defaultHostName)" -ForegroundColor Cyan
        Write-Host "  Resource Group: $($appService.resourceGroup)" -ForegroundColor Cyan
        
        # Test health endpoint
        $healthUrl = "https://$($appService.defaultHostName)/health"
        Write-Host "  Testing health endpoint: $healthUrl" -ForegroundColor Yellow
        try {
            $response = Invoke-WebRequest -Uri $healthUrl -Method GET -TimeoutSec 10
            Write-Host "  ✓ Health check successful: $($response.StatusCode)" -ForegroundColor Green
        } catch {
            Write-Host "  ✗ Health check failed: $($_.Exception.Message)" -ForegroundColor Red
        }
    } else {
        Write-Host "✗ App Service not found: $appServiceName" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Error checking App Service: $appServiceName" -ForegroundColor Red
}

# Test ACR connectivity
Write-Host "`n=== Testing ACR Connectivity ===" -ForegroundColor Cyan
$acrName = "acrqueuehubapi001"

try {
    $acr = az acr show --name $acrName --output json 2>$null | ConvertFrom-Json
    if ($acr) {
        Write-Host "✓ ACR found: $($acr.name)" -ForegroundColor Green
        Write-Host "  Login Server: $($acr.loginServer)" -ForegroundColor Cyan
        Write-Host "  Resource Group: $($acr.resourceGroup)" -ForegroundColor Cyan
        
        # Test ACR login
        Write-Host "  Testing ACR login..." -ForegroundColor Yellow
        try {
            az acr login --name $acrName
            Write-Host "  ✓ ACR login successful" -ForegroundColor Green
        } catch {
            Write-Host "  ✗ ACR login failed: $($_.Exception.Message)" -ForegroundColor Red
        }
    } else {
        Write-Host "✗ ACR not found: $acrName" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Error checking ACR: $acrName" -ForegroundColor Red
}

Write-Host "`n=== CONNECTION TEST SUMMARY ===" -ForegroundColor Green
Write-Host "Current Subscription: $($account.name) ($($account.id))" -ForegroundColor Cyan
Write-Host "Found Resource Groups: $($foundResources -join ', ')" -ForegroundColor Cyan

if ($foundResources.Count -gt 0) {
    Write-Host "`n=== NEXT STEPS ===" -ForegroundColor Yellow
    Write-Host "1. Run setup-azure-logging.ps1 with your subscription ID" -ForegroundColor White
    Write-Host "2. Deploy your application with: deploy-with-logging.ps1" -ForegroundColor White
    Write-Host "3. Get logs with: get-azure-logs.ps1" -ForegroundColor White
    
    Write-Host "`nExample commands:" -ForegroundColor Yellow
    Write-Host ".\scripts\setup-azure-logging.ps1 -SubscriptionId '$($account.id)'" -ForegroundColor Gray
    Write-Host ".\scripts\deploy-with-logging.ps1 -SubscriptionId '$($account.id)'" -ForegroundColor Gray
    Write-Host ".\scripts\get-azure-logs.ps1 -Hours 24" -ForegroundColor Gray
} else {
    Write-Host "`n⚠️  No QueueHub resources found!" -ForegroundColor Red
    Write-Host "Please ensure your resources are deployed or check the resource group names." -ForegroundColor Yellow
} 