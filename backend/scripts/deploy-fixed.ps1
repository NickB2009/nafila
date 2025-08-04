# EuToNaFila Azure Container Deployment Script with Working Database Migrations
# This script runs migrations locally then deploys the container

# Stop on first error
$ErrorActionPreference = "Stop"

# Ensure we're running from the repository root
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$repoRoot = Split-Path -Parent (Split-Path -Parent $scriptPath)
Set-Location $repoRoot
Write-Host "Working from repository root: $repoRoot" -ForegroundColor Yellow

# Configuration
$projectName = "queuehub"
$environment = "p"
$appServiceName = "app-$environment-$projectName-api-001"
$resourceGroup = "rg-$environment-$projectName-core-001"
$acrName = "acrqueuehubapi001"
$imageName = "queuehub-api"
$imageTag = "latest"
$projectPath = "backend/GrandeTech.QueueHub"
$dockerfilePath = "$projectPath/GrandeTech.QueueHub.API/Dockerfile"
$fullImageName = "${acrName}.azurecr.io/${imageName}:${imageTag}"

Write-Host "Starting deployment with database migrations..." -ForegroundColor Green

# ============================================================
# STEP 1: Get Azure Connection String and Run Migrations Locally
# ============================================================
Write-Host "Running database migrations..." -ForegroundColor Yellow

try {
    # Get the connection string from Azure App Service
    Write-Host "Getting connection string from Azure App Service..." -ForegroundColor Yellow
    $connectionString = az webapp config appsettings list --name $appServiceName --resource-group $resourceGroup --query "[?name=='ConnectionStrings__AzureSqlConnection'].value" --output tsv
    
    if ([string]::IsNullOrEmpty($connectionString)) {
        Write-Host "Error: Could not retrieve connection string from App Service." -ForegroundColor Red
        Write-Host "Please ensure the connection string is configured in Azure App Service." -ForegroundColor Yellow
        Write-Host "You can set it manually with:" -ForegroundColor Yellow
        Write-Host "az webapp config appsettings set --name $appServiceName --resource-group $resourceGroup --settings 'ConnectionStrings__AzureSqlConnection=your-connection-string'" -ForegroundColor Cyan
        exit 1
    }
    
    Write-Host "Connection string retrieved successfully" -ForegroundColor Green
    
    # Set environment variable for EF Core
    $env:ConnectionStrings__AzureSqlConnection = $connectionString
    
    # Navigate to API project
    Set-Location "$projectPath/GrandeTech.QueueHub.API"
    Write-Host "Changed to: $(Get-Location)" -ForegroundColor Yellow
    
    # Run migrations with verbose output
    Write-Host "Executing: dotnet ef database update --verbose" -ForegroundColor Yellow
    dotnet ef database update --verbose
    
    if ($LASTEXITCODE -ne 0) {
        throw "Database migration failed with exit code $LASTEXITCODE"
    }
    
    Write-Host "Database migrations completed successfully!" -ForegroundColor Green
    
    # Return to repo root
    Set-Location $repoRoot
    
} catch {
    Write-Host "Error during database migration: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stopping deployment. Please fix migration issues before deploying." -ForegroundColor Red
    exit 1
}

# ============================================================
# STEP 2: Build and Deploy Container (Original Logic)
# ============================================================

# Login to Azure Container Registry
Write-Host "Logging in to Azure Container Registry..." -ForegroundColor Yellow
try {
    az acr login --name $acrName
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to login to ACR"
    }
    Write-Host "ACR login successful" -ForegroundColor Green
} catch {
    Write-Host "Error: Failed to login to ACR. Please check permissions." -ForegroundColor Red
    exit 1
}

# Build the Docker image
Write-Host "Building Docker image..." -ForegroundColor Yellow
try {
    docker build -t $fullImageName -f $dockerfilePath .
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build Docker image"
    }
    Write-Host "Docker image built successfully" -ForegroundColor Green
} catch {
    Write-Host "Error: Failed to build Docker image." -ForegroundColor Red
    exit 1
}

# Push the image to ACR
Write-Host "Pushing image to Azure Container Registry..." -ForegroundColor Yellow
try {
    docker push $fullImageName
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to push Docker image"
    }
    Write-Host "Image pushed successfully" -ForegroundColor Green
} catch {
    Write-Host "Error: Failed to push Docker image to ACR." -ForegroundColor Red
    exit 1
}

# Update the App Service
Write-Host "Updating App Service with new image..." -ForegroundColor Yellow
try {
    az webapp config container set `
        --name $appServiceName `
        --resource-group $resourceGroup `
        --container-image-name $fullImageName
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to update App Service configuration"
    }
    Write-Host "App Service updated successfully" -ForegroundColor Green
} catch {
    Write-Host "Error: Failed to update App Service configuration." -ForegroundColor Red
    exit 1
}

# Restart the App Service
Write-Host "Restarting App Service..." -ForegroundColor Yellow
try {
    az webapp restart --name $appServiceName --resource-group $resourceGroup
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to restart App Service"
    }
    Write-Host "App Service restarted successfully" -ForegroundColor Green
} catch {
    Write-Host "Error: Failed to restart App Service." -ForegroundColor Red
    exit 1
}

# ============================================================
# STEP 3: Success Message and URLs
# ============================================================

Write-Host "" -ForegroundColor White
Write-Host "DEPLOYMENT COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green
Write-Host "" -ForegroundColor White

# Get the actual application URL from Azure
try {
    $appUrl = az webapp show --name $appServiceName --resource-group $resourceGroup --query "defaultHostName" --output tsv
    if ($appUrl) {
        Write-Host "Application URL: https://$appUrl" -ForegroundColor Cyan
        Write-Host "Swagger Documentation: https://$appUrl/swagger" -ForegroundColor Cyan
        Write-Host "Health Check: https://$appUrl/health" -ForegroundColor Cyan
    } else {
        Write-Host "Application URL: https://$appServiceName.azurewebsites.net" -ForegroundColor Cyan
    }
} catch {
    Write-Host "Application URL: https://$appServiceName.azurewebsites.net" -ForegroundColor Cyan
}

Write-Host "" -ForegroundColor White
Write-Host "Database migrations applied:" -ForegroundColor Green
Write-Host "   - AddWeeklyBusinessHours (Complete)" -ForegroundColor Green  
Write-Host "   - RemoveOldBusinessHours (Complete)" -ForegroundColor Green
Write-Host "   - RefactorToJsonColumn (Complete)" -ForegroundColor Green
Write-Host "" -ForegroundColor White
Write-Host "Sunday business hours are now configurable via API!" -ForegroundColor Yellow
Write-Host "Use endpoint: PUT /api/locations/{id}/weekly-hours" -ForegroundColor Yellow
Write-Host "" -ForegroundColor White