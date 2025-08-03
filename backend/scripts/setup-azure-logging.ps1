# Setup Azure Application Insights Logging for QueueHub API
# This script enables comprehensive logging and monitoring for the production API

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
    [string]$Location = "East US"
)

# Stop on first error
$ErrorActionPreference = "Stop"

Write-Host "Setting up Azure Application Insights logging for QueueHub API..." -ForegroundColor Green
Write-Host "Subscription ID: $SubscriptionId" -ForegroundColor Yellow
Write-Host "Resource Group: $ResourceGroupName" -ForegroundColor Yellow
Write-Host "App Service: $AppServiceName" -ForegroundColor Yellow
Write-Host "App Insights: $AppInsightsName" -ForegroundColor Yellow
Write-Host "Location: $Location" -ForegroundColor Yellow

# Set the subscription
Write-Host "Setting Azure subscription..." -ForegroundColor Yellow
az account set --subscription $SubscriptionId

# Check if Application Insights already exists
Write-Host "Checking if Application Insights resource exists..." -ForegroundColor Yellow
$existingAppInsights = az monitor app-insights component show --app $AppInsightsName --resource-group $ResourceGroupName --query "name" --output tsv 2>$null

if ($existingAppInsights) {
    Write-Host "Application Insights '$AppInsightsName' already exists." -ForegroundColor Yellow
} else {
    Write-Host "Creating Application Insights resource..." -ForegroundColor Yellow
    az monitor app-insights component create --app $AppInsightsName --location $Location --resource-group $ResourceGroupName --application-type web
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error: Failed to create Application Insights resource." -ForegroundColor Red
        exit 1
    }
    Write-Host "Application Insights resource created successfully." -ForegroundColor Green
}

# Get Application Insights instrumentation key
Write-Host "Getting Application Insights instrumentation key..." -ForegroundColor Yellow
$instrumentationKey = az monitor app-insights component show --app $AppInsightsName --resource-group $ResourceGroupName --query "instrumentationKey" --output tsv

if (-not $instrumentationKey) {
    Write-Host "Error: Failed to get Application Insights instrumentation key." -ForegroundColor Red
    exit 1
}

Write-Host "Instrumentation Key: $instrumentationKey" -ForegroundColor Cyan

# Configure App Service to use Application Insights
Write-Host "Configuring App Service to use Application Insights..." -ForegroundColor Yellow
az webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to configure App Service with Application Insights." -ForegroundColor Red
    exit 1
}

# Enable Application Insights extension for the App Service
Write-Host "Enabling Application Insights extension for App Service..." -ForegroundColor Yellow
az webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings "ApplicationInsightsAgent_EXTENSION_VERSION=~3"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to enable Application Insights extension." -ForegroundColor Red
    exit 1
}

# Enable detailed logging
Write-Host "Enabling detailed logging settings..." -ForegroundColor Yellow
az webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings "WEBSITE_HTTPLOGGING_RETENTION_DAYS=7"

# Enable container logging
Write-Host "Enabling container logging..." -ForegroundColor Yellow
az webapp log config --name $AppServiceName --resource-group $ResourceGroupName --web-server-logging filesystem

# Enable detailed error logging
Write-Host "Enabling detailed error logging..." -ForegroundColor Yellow
az webapp log config --name $AppServiceName --resource-group $ResourceGroupName --detailed-error-messages true

# Enable failed request tracing
Write-Host "Enabling failed request tracing..." -ForegroundColor Yellow
az webapp log config --name $AppServiceName --resource-group $ResourceGroupName --failed-request-tracing true

# Get the Application Insights URL
$appInsightsUrl = az monitor app-insights component show --app $AppInsightsName --resource-group $ResourceGroupName --query "applicationId" --output tsv
$appInsightsPortalUrl = "https://portal.azure.com/#@/resource/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Insights/components/$AppInsightsName"

Write-Host "`n=== AZURE LOGGING SETUP COMPLETE ===" -ForegroundColor Green
Write-Host "Application Insights Name: $AppInsightsName" -ForegroundColor Cyan
Write-Host "Instrumentation Key: $instrumentationKey" -ForegroundColor Cyan
Write-Host "Application Insights Portal URL: $appInsightsPortalUrl" -ForegroundColor Cyan

Write-Host "`n=== NEXT STEPS ===" -ForegroundColor Yellow
Write-Host "1. Update your application configuration with the instrumentation key" -ForegroundColor White
Write-Host "2. Deploy your application with Application Insights SDK" -ForegroundColor White
Write-Host "3. Monitor logs in Azure Portal: $appInsightsPortalUrl" -ForegroundColor White
Write-Host "4. Set up alerts for critical errors" -ForegroundColor White

Write-Host "`n=== LOGGING FEATURES ENABLED ===" -ForegroundColor Green
Write-Host "✓ Application Insights integration" -ForegroundColor Green
Write-Host "✓ Web server logging (7 days retention)" -ForegroundColor Green
Write-Host "✓ Detailed error messages" -ForegroundColor Green
Write-Host "✓ Failed request tracing" -ForegroundColor Green
Write-Host "✓ Container logging" -ForegroundColor Green 