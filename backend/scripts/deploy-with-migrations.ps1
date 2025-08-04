# EuToNaFila Azure Container Deployment Script with Database Migrations
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
$acrName = "acrqueuehubapi001"
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

# ============================================================
# NEW: Run Database Migrations Before App Update
# ============================================================
Write-Host "Running database migrations..." -ForegroundColor Yellow
try {
    # Get the connection string from Azure App Service
    $connectionString = az webapp config appsettings list --name $appServiceName --resource-group $resourceGroup --query "[?name=='ConnectionStrings__AzureSqlConnection'].value" --output tsv
    
    if ([string]::IsNullOrEmpty($connectionString)) {
        Write-Host "Warning: Could not retrieve connection string from App Service. Attempting to run migrations from container..." -ForegroundColor Yellow
        
        # Alternative: Run migrations from a temporary container
        Write-Host "Running migrations from temporary container..." -ForegroundColor Yellow
        docker run --rm $fullImageName dotnet ef database update --verbose
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to run database migrations from container"
        }
    } else {
        # Run migrations locally with Azure connection string
        Write-Host "Running migrations with Azure connection string..." -ForegroundColor Yellow
        Set-Location "$projectPath/GrandeTech.QueueHub.API"
        $env:ConnectionStrings__AzureSqlConnection = $connectionString
        dotnet ef database update --verbose
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to run database migrations"
        }
        Set-Location $repoRoot
    }
    
    Write-Host "Database migrations completed successfully!" -ForegroundColor Green
} catch {
    Write-Host "Error: Failed to run database migrations. Deployment will continue but database may be out of sync." -ForegroundColor Red
    Write-Host "You may need to run migrations manually: dotnet ef database update" -ForegroundColor Yellow
    # Continue with deployment even if migrations fail
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
        Write-Host "Application URL: https://$appServiceName.azurewebsites.net" -ForegroundColor Yellow
    }
} catch {
    Write-Host "Application URL: https://$appServiceName.azurewebsites.net" -ForegroundColor Yellow
}

Write-Host "🎉 Deployment complete! Your JSON business hours are now live!" -ForegroundColor Green