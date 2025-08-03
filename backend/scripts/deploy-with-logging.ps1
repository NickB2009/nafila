# Deploy QueueHub API with Azure Logging Setup
# This script sets up logging and deploys the application

param(
    [Parameter(Mandatory=$true)]
    [string]$SubscriptionId,
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroupName = "rg-p-queuehub-core-001",
    
    [Parameter(Mandatory=$false)]
    [string]$AppServiceName = "app-p-queuehub-api-001",
    
    [Parameter(Mandatory=$false)]
    [string]$AppInsightsName = "appi-p-queuehub-api-001",
    
    [Parameter(Mandatory=$false)]
    [string]$AcrName = "acrqueuehubapi001",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipLoggingSetup,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipDeployment
)

# Stop on first error
$ErrorActionPreference = "Stop"

Write-Host "Deploying QueueHub API with Azure Logging..." -ForegroundColor Green
Write-Host "Subscription ID: $SubscriptionId" -ForegroundColor Yellow
Write-Host "Resource Group: $ResourceGroupName" -ForegroundColor Yellow
Write-Host "App Service: $AppServiceName" -ForegroundColor Yellow
Write-Host "App Insights: $AppInsightsName" -ForegroundColor Yellow
Write-Host "ACR: $AcrName" -ForegroundColor Yellow

# Set the subscription
Write-Host "Setting Azure subscription..." -ForegroundColor Yellow
az account set --subscription $SubscriptionId

# Step 1: Setup Azure Logging (if not skipped)
if (-not $SkipLoggingSetup) {
    Write-Host "`n=== Step 1: Setting up Azure Logging ===" -ForegroundColor Cyan
    
    # Check if Application Insights exists
    $existingAppInsights = az monitor app-insights component show --app $AppInsightsName --resource-group $ResourceGroupName --query "name" --output tsv 2>$null
    
    if (-not $existingAppInsights) {
        Write-Host "Creating Application Insights resource..." -ForegroundColor Yellow
        az monitor app-insights component create --app $AppInsightsName --location "East US" --resource-group $ResourceGroupName --application-type web
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error: Failed to create Application Insights resource." -ForegroundColor Red
            exit 1
        }
        Write-Host "Application Insights resource created successfully." -ForegroundColor Green
    } else {
        Write-Host "Application Insights '$AppInsightsName' already exists." -ForegroundColor Yellow
    }
    
    # Get Application Insights instrumentation key
    Write-Host "Getting Application Insights instrumentation key..." -ForegroundColor Yellow
    $instrumentationKey = az monitor app-insights component show --app $AppInsightsName --resource-group $ResourceGroupName --query "instrumentationKey" --output tsv
    
    if (-not $instrumentationKey) {
        Write-Host "Error: Failed to get Application Insights instrumentation key." -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Instrumentation Key: $instrumentationKey" -ForegroundColor Cyan
    
    # Configure App Service with Application Insights
    Write-Host "Configuring App Service with Application Insights..." -ForegroundColor Yellow
    az webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey"
    az webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings "ApplicationInsightsAgent_EXTENSION_VERSION=~3"
    
    # Enable detailed logging
    Write-Host "Enabling detailed logging settings..." -ForegroundColor Yellow
    az webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings "WEBSITE_HTTPLOGGING_RETENTION_DAYS=7"
    az webapp log config --name $AppServiceName --resource-group $ResourceGroupName --web-server-logging filesystem
    az webapp log config --name $AppServiceName --resource-group $ResourceGroupName --detailed-error-messages true
    az webapp log config --name $AppServiceName --resource-group $ResourceGroupName --failed-request-tracing true
    
    Write-Host "Azure logging setup completed successfully!" -ForegroundColor Green
} else {
    Write-Host "Skipping Azure logging setup..." -ForegroundColor Yellow
}

# Step 2: Deploy the application (if not skipped)
if (-not $SkipDeployment) {
    Write-Host "`n=== Step 2: Deploying Application ===" -ForegroundColor Cyan
    
    # Ensure we're running from the repository root
    $scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
    $repoRoot = Split-Path -Parent (Split-Path -Parent $scriptPath)
    Set-Location $repoRoot
    Write-Host "Working from repository root: $repoRoot" -ForegroundColor Yellow
    
    # Configuration
    $imageName = "queuehub-api"
    $imageTag = "latest"
    $projectPath = "backend/GrandeTech.QueueHub"
    $dockerfilePath = "$projectPath/GrandeTech.QueueHub.API/Dockerfile"
    $fullImageName = "${AcrName}.azurecr.io/${imageName}:${imageTag}"
    
    # Login to Azure Container Registry
    Write-Host "Logging in to Azure Container Registry..." -ForegroundColor Yellow
    try {
        az acr login --name $AcrName
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to login to ACR"
        }
    } catch {
        Write-Host "Error: Failed to login to ACR. Please check if the ACR name is correct and you have proper permissions." -ForegroundColor Red
        exit 1
    }
    
    # Build the Docker image
    Write-Host "Building Docker image..." -ForegroundColor Yellow
    Write-Host "Dockerfile path: $dockerfilePath" -ForegroundColor Yellow
    Write-Host "Build context: $(Get-Location)" -ForegroundColor Yellow
    try {
        docker build -t $fullImageName -f $dockerfilePath .
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to build Docker image"
        }
    } catch {
        Write-Host "Error: Failed to build Docker image. Please check if Docker is running and the Dockerfile exists." -ForegroundColor Red
        exit 1
    }
    
    # Push the image to ACR
    Write-Host "Pushing image to Azure Container Registry..." -ForegroundColor Yellow
    try {
        docker push $fullImageName
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to push Docker image"
        }
    } catch {
        Write-Host "Error: Failed to push Docker image to ACR. Please check your ACR permissions." -ForegroundColor Red
        exit 1
    }
    
    # Update the App Service to use the new image
    Write-Host "Updating App Service with new image..." -ForegroundColor Yellow
    try {
        az webapp config container set `
            --name $AppServiceName `
            --resource-group $ResourceGroupName `
            --container-image-name $fullImageName
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to update App Service configuration"
        }
    } catch {
        Write-Host "Error: Failed to update App Service configuration. Please check your App Service permissions." -ForegroundColor Red
        exit 1
    }
    
    # Restart the App Service
    Write-Host "Restarting App Service..." -ForegroundColor Yellow
    try {
        az webapp restart --name $AppServiceName --resource-group $ResourceGroupName
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to restart App Service"
        }
    } catch {
        Write-Host "Error: Failed to restart App Service. Please check your App Service permissions." -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Application deployment completed successfully!" -ForegroundColor Green
} else {
    Write-Host "Skipping application deployment..." -ForegroundColor Yellow
}

# Step 3: Verify deployment
Write-Host "`n=== Step 3: Verifying Deployment ===" -ForegroundColor Cyan

# Get the application URL
try {
    $appUrl = az webapp show --name $AppServiceName --resource-group $ResourceGroupName --query "defaultHostName" --output tsv
    if ($appUrl) {
        Write-Host "Application URL: https://$appUrl" -ForegroundColor Cyan
        Write-Host "Swagger Documentation: https://$appUrl/swagger" -ForegroundColor Cyan
        Write-Host "Health Check: https://$appUrl/health" -ForegroundColor Cyan
        
        # Test health endpoint
        Write-Host "Testing health endpoint..." -ForegroundColor Yellow
        try {
            $response = Invoke-WebRequest -Uri "https://$appUrl/health" -Method GET -TimeoutSec 30
            Write-Host "Health check successful: $($response.StatusCode)" -ForegroundColor Green
        } catch {
            Write-Host "Health check failed: $($_.Exception.Message)" -ForegroundColor Red
            Write-Host "The application may still be starting up. Please wait a few minutes and try again." -ForegroundColor Yellow
        }
    }
} catch {
    Write-Host "Warning: Could not retrieve application URL" -ForegroundColor Yellow
}

# Get Application Insights URL
$appInsightsPortalUrl = "https://portal.azure.com/#@/resource/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Insights/components/$AppInsightsName"

Write-Host "`n=== DEPLOYMENT COMPLETE ===" -ForegroundColor Green
Write-Host "Application Insights Portal: $appInsightsPortalUrl" -ForegroundColor Cyan

Write-Host "`n=== NEXT STEPS ===" -ForegroundColor Yellow
Write-Host "1. Monitor logs in Azure Portal: $appInsightsPortalUrl" -ForegroundColor White
Write-Host "2. Get logs locally: .\scripts\get-azure-logs.ps1 -Hours 24" -ForegroundColor White
Write-Host "3. Set up alerts for critical errors in Application Insights" -ForegroundColor White
Write-Host "4. Monitor application performance and usage" -ForegroundColor White

Write-Host "`n=== LOGGING FEATURES ENABLED ===" -ForegroundColor Green
Write-Host "✓ Application Insights integration" -ForegroundColor Green
Write-Host "✓ Request/response logging" -ForegroundColor Green
Write-Host "✓ Performance monitoring" -ForegroundColor Green
Write-Host "✓ Error tracking" -ForegroundColor Green
Write-Host "✓ Custom business events" -ForegroundColor Green
Write-Host "Database operation logging" -ForegroundColor Green 