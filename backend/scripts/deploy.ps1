# EuToNaFila Azure Container Deployment Script
# This script builds and deploys the Docker container to ACR and updates the App Service

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
$acrName = "acrqueuehubapi001"  # Using the exact name from README.md
$imageName = "queuehub-api"
$imageTag = "latest"
$projectPath = "backend/GrandeTech.QueueHub"
$dockerfilePath = "$projectPath/GrandeTech.QueueHub.API/Dockerfile"
$fullImageName = "${acrName}.azurecr.io/${imageName}:${imageTag}"

Write-Host "Starting container deployment to Azure..." -ForegroundColor Green
Write-Host "Using ACR: $acrName" -ForegroundColor Yellow

# Login to Azure Container Registry
Write-Host "Logging in to Azure Container Registry..." -ForegroundColor Yellow
try {
    az acr login --name $acrName
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
    # Build from the repository root with the correct context
    docker build -t $fullImageName -f $dockerfilePath .
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build Docker image"
    }
} catch {
    Write-Host "Error: Failed to build Docker image. Please check if Docker is running and the Dockerfile exists." -ForegroundColor Red
    Write-Host "Current directory: $(Get-Location)" -ForegroundColor Yellow
    Write-Host "Dockerfile path: $dockerfilePath" -ForegroundColor Yellow
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
        --name $appServiceName `
        --resource-group $resourceGroup `
        --container-image-name $fullImageName
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to update App Service configuration"
    }
} catch {
    Write-Host "Error: Failed to update App Service configuration. Please check your App Service permissions." -ForegroundColor Red
    exit 1
}

# Restart the App Service to ensure it picks up the new image
Write-Host "Restarting App Service..." -ForegroundColor Yellow
try {
    az webapp restart --name $appServiceName --resource-group $resourceGroup
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to restart App Service"
    }
} catch {
    Write-Host "Error: Failed to restart App Service. Please check your App Service permissions." -ForegroundColor Red
    exit 1
}

Write-Host "Deployment completed successfully!" -ForegroundColor Green

# Get the actual application URL from Azure
try {
    $appUrl = az webapp show --name $appServiceName --resource-group $resourceGroup --query "defaultHostName" --output tsv
    if ($appUrl) {
        Write-Host "Application URL: https://$appUrl" -ForegroundColor Cyan
        Write-Host "Swagger Documentation: https://$appUrl/swagger" -ForegroundColor Cyan
        Write-Host "Health Check: https://$appUrl/health" -ForegroundColor Cyan
    } else {
        Write-Host "Application URL: https://$appServiceName.azurewebsites.net (Note: Actual URL may include unique Azure identifiers)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "Application URL: https://$appServiceName.azurewebsites.net (Note: Actual URL may include unique Azure identifiers)" -ForegroundColor Yellow
} 